using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace AdvancedPeopleSystem
{
    public class CharacterCustomizationCombiner
    {
        class MeshInstance
        {
            public Dictionary<Material, List<CombineInstanceWithSM>>
                                            combine_instances = new Dictionary<Material, List<CombineInstanceWithSM>>();
            public List<Material> unique_materials = new List<Material>();
            public Mesh combined_new_mesh = new Mesh();
            public List<Vector3> combined_vertices = new List<Vector3>();
            public List<Vector2> combined_uv = new List<Vector2>();
            public List<Vector2> combined_uv2 = new List<Vector2>();
            public List<Vector2> combined_uv3 = new List<Vector2>();
            public List<Vector2> combined_uv4 = new List<Vector2>();

            public List<Vector3> normals = new List<Vector3>();
            public List<Vector4> tangents = new List<Vector4>();

            public Dictionary<Material, List<int>>
                                            combined_submesh_indices = new Dictionary<Material, List<int>>();
            public List<BoneWeight> combined_bone_weights = new List<BoneWeight>();
            public List<string> blendShapeNames = new List<string>();
            public List<float> blendShapeValues = new List<float>();
            public Dictionary<Mesh, int> vertex_offset_map = new Dictionary<Mesh, int>();
            public int vertex_index_offset = 0;
            public int current_material_index = 0;
        }
        struct CombineInstanceWithSM
        {
            public SkinnedMeshRenderer skinnedMesh;
            public CombineInstance instance;
        }
        struct BlendWeightData
        {
            public Vector3[] deltaVerts;
            public Vector3[] deltaNormals;
            public Vector3[] deltaTangents;
        }

        class CharacterBakeQueue
        {
            public CharacterCustomization character;
            public bool bakeBlendshapes;
            public GameObject exportInCustomObject;
            public float blendshapeAddDelay;
            public Action<List<SkinnedMeshRenderer>> callback;
        }

        static Matrix4x4[] bindPoses;
        static List<MeshInstance> LODMeshInstances;

        static CharacterCustomization currentCharacter;

        static bool useExportToAnotherObject = false;

        static bool BlendshapeTransferWork = false;

        static Action<List<SkinnedMeshRenderer>> _callback;

        static List<SkinnedMeshRenderer> returnSkinnedMeshes = new List<SkinnedMeshRenderer>();

        static Queue<CharacterBakeQueue> characterBakeQueue = new Queue<CharacterBakeQueue>();

        public static void ClearBakeQueue()
        {
            characterBakeQueue.Clear();
        }

        public static void AddBakeToQueue(CharacterCustomization character, bool bakeBlendshapes = false, GameObject exportInCustomObject = null, float blendshapeAddDelay = 0.001f, Action<List<SkinnedMeshRenderer>> callback = null)
        {
            if (character.IsBaked())
                character.ClearBake();
            if(characterBakeQueue == null)
            {
                characterBakeQueue = new Queue<CharacterBakeQueue>();
            }
            CharacterBakeQueue newBakeQuest = new CharacterBakeQueue();
            newBakeQuest.character = character;
            newBakeQuest.bakeBlendshapes = bakeBlendshapes;
            newBakeQuest.exportInCustomObject = exportInCustomObject;
            newBakeQuest.blendshapeAddDelay = blendshapeAddDelay;
            newBakeQuest.callback = callback;
            characterBakeQueue.Enqueue(newBakeQuest);
            if (characterBakeQueue.Count == 1)
            {        
                EnqueueCharacterBake();
            }
        }

        public static void DequeueFirst()
        {
            if(characterBakeQueue.Count> 0)
            {
                characterBakeQueue.Dequeue();
            }
        }

        public static void EnqueueCharacterBake()
        {
            if (characterBakeQueue.Count > 0)
            {            
                var enqueueCharacter = characterBakeQueue.Peek();

                if (enqueueCharacter != null)
                {                  
                    MakeCombinedMeshes(enqueueCharacter.character, enqueueCharacter.bakeBlendshapes, enqueueCharacter.exportInCustomObject, enqueueCharacter.blendshapeAddDelay, enqueueCharacter.callback);                 
                }
            }
        }

        public static List<SkinnedMeshRenderer> MakeCombinedMeshes(CharacterCustomization character, bool bakeBlendshapes = false, GameObject exportInCustomObject = null, float blendshapeAddDelay = 0.001f, Action<List<SkinnedMeshRenderer>> callback = null)
        {
            returnSkinnedMeshes.Clear();
            if (character.IsBaked())
            {
                Debug.LogError("Character is already combined!");
                return null;
            }
            if (callback != null)
                _callback = callback;


            
            BlendshapeTransferWork = false;
            useExportToAnotherObject = (exportInCustomObject != null);
         
            if(!useExportToAnotherObject)
            character.CurrentCombinerState = CombinerState.InProgressCombineMesh;

            currentCharacter = character;


            SkinnedMeshRenderer bodyMesh = character.GetCharacterPart("Head").skinnedMesh[0];
            bindPoses = bodyMesh.sharedMesh.bindposes;
            LODMeshInstances = new List<MeshInstance>();
            void SelectMeshes(SkinnedMeshRenderer mesh, int LOD)
            {
                if (mesh != null)
                {
                    for (int m = 0; m < mesh.sharedMaterials.Length; m++)
                    {
                        Material mat = mesh.sharedMaterials[m];

                        Mesh sharedMesh = mesh.sharedMesh;

                        if (sharedMesh == null || mesh.gameObject.activeSelf == false || mesh.enabled == false || sharedMesh.vertexCount == 0 || sharedMesh.subMeshCount - 1 < m)
                            continue;

                        if (!LODMeshInstances[LOD].combine_instances.ContainsKey(mat))
                        {
                            LODMeshInstances[LOD].combine_instances.Add(mat, new List<CombineInstanceWithSM>());
                            LODMeshInstances[LOD].unique_materials.Add(mat);
                        }

                        CombineInstanceWithSM instanceWithSM = new CombineInstanceWithSM();

                        CombineInstance combineInstance = new CombineInstance();
                        combineInstance.transform = Matrix4x4.identity;

                        combineInstance.subMeshIndex = m;
                        combineInstance.mesh = sharedMesh;

                        instanceWithSM.instance = combineInstance;
                        instanceWithSM.skinnedMesh = mesh;

                        LODMeshInstances[LOD].combine_instances[mat].Add(instanceWithSM);
                    }
                }
            }

            for (int i = 0; i < (character.MaxLODLevels - character.MinLODLevels) + 1; i++)
            {
                LODMeshInstances.Add(new MeshInstance());
                foreach(var m in character.GetAllMeshesByLod(i))
                {
                    SelectMeshes(m, i);
                }
            }


            SkinnedMeshRenderer meshForCombine = character.GetCharacterPart("Combined").skinnedMesh[0];
            List<SkinnedMeshRenderer> newObjectsForCombine = character.GetCharacterPart("Combined").skinnedMesh;

            if (exportInCustomObject != null)
            { 
                List<SkinnedMeshRenderer> skinnedMeshRenderersForExport = new List<SkinnedMeshRenderer>();

                for (int i = 0; i < (character.MaxLODLevels-character.MinLODLevels)+1; i++)
                {
                    SkinnedMeshRenderer mesh = GameObject.Instantiate(meshForCombine, exportInCustomObject.transform);
                    skinnedMeshRenderersForExport.Add(mesh);
                }
                newObjectsForCombine = skinnedMeshRenderersForExport;
            }

            for (int i = 0; i < LODMeshInstances.Count; i++)
            {
                MeshInstance meshInstance = LODMeshInstances[i]; //Base LOD Instance

                for (int t = 0; t < meshInstance.unique_materials.Count; t++)
                {
                    Material mat = meshInstance.unique_materials[t];
                    List<CombineInstanceWithSM> combineInstances = meshInstance.combine_instances[meshInstance.unique_materials[t]]; // Combine instance for each material

                    for (int m = 0; m < combineInstances.Count; m++)
                    {
                        CombineInstanceWithSM combineInstance = combineInstances[m];

                        if (!meshInstance.vertex_offset_map.ContainsKey(combineInstance.instance.mesh))
                        {
                            meshInstance.combined_vertices.AddRange(combineInstance.instance.mesh.vertices);

                            if (combineInstance.instance.mesh.uv.Length == 0)
                                meshInstance.combined_uv.AddRange(new Vector2[combineInstance.instance.mesh.vertexCount]);
                            else
                                meshInstance.combined_uv.AddRange(combineInstance.instance.mesh.uv);

                            if (combineInstance.instance.mesh.uv2.Length == 0)
                                meshInstance.combined_uv2.AddRange(new Vector2[combineInstance.instance.mesh.vertexCount]);
                            else
                                meshInstance.combined_uv2.AddRange(combineInstance.instance.mesh.uv2);

                            if (combineInstance.instance.mesh.uv3.Length == 0)
                                meshInstance.combined_uv3.AddRange(new Vector2[combineInstance.instance.mesh.vertexCount]);
                            else
                                meshInstance.combined_uv3.AddRange(combineInstance.instance.mesh.uv3);

                            meshInstance.normals.AddRange(combineInstance.instance.mesh.normals);

                            meshInstance.combined_bone_weights.AddRange(combineInstance.instance.mesh.boneWeights);
                            meshInstance.vertex_offset_map[combineInstance.instance.mesh] = meshInstance.vertex_index_offset;
                            meshInstance.vertex_index_offset += combineInstance.instance.mesh.vertexCount;
                        }

                        int currentOffset = meshInstance.vertex_offset_map[combineInstance.instance.mesh];

                        int[] indices = combineInstance.instance.mesh.GetTriangles(combineInstance.instance.subMeshIndex);

                        for (int d = 0; d < indices.Length; d++)
                            indices[d] += currentOffset;
                        

                        if (!meshInstance.combined_submesh_indices.ContainsKey(mat))
                            meshInstance.combined_submesh_indices.Add(mat, indices.ToList());
                        else
                            meshInstance.combined_submesh_indices[mat].AddRange(indices);


                        for (int s = 0; s < combineInstance.instance.mesh.blendShapeCount; s++)
                        {
                            string shape_name = combineInstance.instance.mesh.GetBlendShapeName(s);

                            if (!meshInstance.blendShapeNames.Contains(shape_name))
                            {
                                meshInstance.blendShapeNames.Add(shape_name);
                                meshInstance.blendShapeValues.Add(combineInstance.skinnedMesh.GetBlendShapeWeight(s));
                            }
                        }
                    }
                }

                //Bake Blendshapes
                if (bakeBlendshapes)
                {
                    int offset;
                    try
                    {
                        for (int bs = 0; bs < meshInstance.blendShapeNames.Count; bs++)
                        {
                            offset = 0;

                            BlendWeightData combWeights = new BlendWeightData();
                            combWeights.deltaNormals = new Vector3[meshInstance.combined_vertices.Count];
                            combWeights.deltaVerts = new Vector3[meshInstance.combined_vertices.Count];

                            foreach (KeyValuePair<Material, List<CombineInstanceWithSM>> combine_instance in meshInstance.combine_instances)
                            {
                                foreach (CombineInstanceWithSM combine in combine_instance.Value)
                                {
                                    if (combine.instance.subMeshIndex > 0)
                                        continue;

                                    int vcount = combine.instance.mesh.vertexCount;

                                    Vector3[] deltaVerts = new Vector3[vcount];
                                    Vector3[] deltaNormals = new Vector3[vcount];

                                    //If this mesh has data, pack that. Otherwise the deltas will be zero, and those will be packed
                                    int bi = combine.instance.mesh.GetBlendShapeIndex(meshInstance.blendShapeNames[bs]);
                                    if (bi != -1)
                                    {
                                        int index = combine.instance.mesh.GetBlendShapeIndex(meshInstance.blendShapeNames[bs]);
                                        combine.instance.mesh.GetBlendShapeFrameVertices(index, combine.instance.mesh.GetBlendShapeFrameCount(index) - 1, deltaVerts, deltaNormals, null);

                                        System.Array.Copy(deltaVerts, 0, combWeights.deltaVerts, offset, vcount);
                                        System.Array.Copy(deltaNormals, 0, combWeights.deltaNormals, offset, vcount);
                                    }
                                    offset += vcount;
                                }
                            }
                            if (meshInstance.blendShapeValues[bs] != 0)
                            {
                                for (int vi = 0; vi < meshInstance.combined_vertices.Count; vi++)
                                {
                                    meshInstance.combined_vertices[vi] += combWeights.deltaVerts[vi] * (meshInstance.blendShapeValues[bs] / 100f);
                                    meshInstance.normals[vi] += combWeights.deltaNormals[vi] * (meshInstance.blendShapeValues[bs] / 100f);
                                }
                            }

                        }

                    }
                    catch (Exception ex) { Debug.LogError(ex); }
                }
                meshInstance.combined_new_mesh.vertices = meshInstance.combined_vertices.ToArray();

                meshInstance.combined_new_mesh.uv = meshInstance.combined_uv.ToArray();

                if (meshInstance.combined_uv2.Count > 0)
                    meshInstance.combined_new_mesh.uv2 = meshInstance.combined_uv2.ToArray();
                if (meshInstance.combined_uv3.Count > 0)
                    meshInstance.combined_new_mesh.uv3 = meshInstance.combined_uv3.ToArray();
                if (meshInstance.combined_uv4.Count > 0)
                    meshInstance.combined_new_mesh.uv4 = meshInstance.combined_uv4.ToArray();

                meshInstance.combined_new_mesh.boneWeights = meshInstance.combined_bone_weights.ToArray();
                meshInstance.combined_new_mesh.name = string.Format("APP_CombinedMesh_lod{0}", i);
                meshInstance.combined_new_mesh.subMeshCount = meshInstance.unique_materials.Count;
                for (int subMeshIndex = 0; subMeshIndex < meshInstance.unique_materials.Count; ++subMeshIndex)
                {
                    meshInstance.combined_new_mesh.SetTriangles(meshInstance.combined_submesh_indices[meshInstance.unique_materials[subMeshIndex]], subMeshIndex);
                }
                meshInstance.combined_new_mesh.SetNormals(meshInstance.normals);
                meshInstance.combined_new_mesh.RecalculateTangents();

                if (!useExportToAnotherObject)
                {
                    if (character.CurrentCombinerState != CombinerState.InProgressBlendshapeTransfer)
                        character.CurrentCombinerState = CombinerState.InProgressBlendshapeTransfer;
                }
                if(!bakeBlendshapes)
                    character.StartCoroutine(BlendshapeTransfer(meshInstance, blendshapeAddDelay, newObjectsForCombine[i], i, exportInCustomObject == null));
            }

            for (int sm = 0; sm < newObjectsForCombine.Count; sm++)
            {
                newObjectsForCombine[sm].name = string.Format("combinemesh_lod{0}", sm);
                newObjectsForCombine[sm].sharedMesh = LODMeshInstances[sm].combined_new_mesh;
                newObjectsForCombine[sm].sharedMesh.bindposes = bindPoses;
                newObjectsForCombine[sm].sharedMaterials = LODMeshInstances[sm].unique_materials.ToArray();


                newObjectsForCombine[sm].updateWhenOffscreen = true;                   
                  
            }
                
            returnSkinnedMeshes.AddRange(newObjectsForCombine);
            BlendshapeTransferWork = true;
            if(bakeBlendshapes)
            {
                if (!useExportToAnotherObject)
                    character.CurrentCombinerState = CombinerState.Combined;
                _callback?.Invoke(returnSkinnedMeshes);

                _callback = null;

                DequeueFirst();
                EnqueueCharacterBake();
            }
            return returnSkinnedMeshes;
        }
        static IEnumerator BlendshapeTransfer(MeshInstance meshInstance, float waitTime, SkinnedMeshRenderer smr, int lod, bool yieldUse = true)
        {
            yield return new WaitWhile(() => BlendshapeTransferWork == false);

            CharacterCustomization characterSystem = currentCharacter;

            int offset;

            for (int bs = 0; bs < meshInstance.blendShapeNames.Count; bs++)
            {
                offset = 0;

                BlendWeightData combWeights = new BlendWeightData();
                combWeights.deltaNormals = new Vector3[meshInstance.combined_new_mesh.vertexCount];
                combWeights.deltaTangents = new Vector3[meshInstance.combined_new_mesh.vertexCount];
                combWeights.deltaVerts = new Vector3[meshInstance.combined_new_mesh.vertexCount];

                foreach (KeyValuePair<Material, List<CombineInstanceWithSM>> combine_instance in meshInstance.combine_instances)
                {
                    foreach (CombineInstanceWithSM combine in combine_instance.Value)
                    {
                        if (combine.instance.subMeshIndex > 0)
                            continue;

                        int vcount = combine.instance.mesh.vertexCount;

                        Vector3[] deltaVerts = new Vector3[vcount];
                        Vector3[] deltaNormals = new Vector3[vcount];
                        Vector3[] deltaTangents = new Vector3[vcount];

                        //If this mesh has data, pack that. Otherwise the deltas will be zero, and those will be packed
                        int bi = combine.instance.mesh.GetBlendShapeIndex(meshInstance.blendShapeNames[bs]);
                        if (bi != -1)
                        {
                            int index = combine.instance.mesh.GetBlendShapeIndex(meshInstance.blendShapeNames[bs]);
                            combine.instance.mesh.GetBlendShapeFrameVertices(index, combine.instance.mesh.GetBlendShapeFrameCount(index) - 1, deltaVerts, deltaNormals, deltaTangents);

                            System.Array.Copy(deltaVerts, 0, combWeights.deltaVerts, offset, vcount);
                            System.Array.Copy(deltaNormals, 0, combWeights.deltaNormals, offset, vcount);
                            System.Array.Copy(deltaTangents, 0, combWeights.deltaTangents, offset, vcount);
                        }
                        offset += vcount;
                    }
                }

                smr.sharedMesh.AddBlendShapeFrame(meshInstance.blendShapeNames[bs], 100, combWeights.deltaVerts, combWeights.deltaNormals, combWeights.deltaTangents);
                
                smr.SetBlendShapeWeight(bs, meshInstance.blendShapeValues[bs]);

                if (waitTime > 0 && yieldUse)
                    yield return new WaitForSecondsRealtime(waitTime);

            }


            if (lod == (characterSystem.MaxLODLevels - characterSystem.MinLODLevels))
            {
                if (!useExportToAnotherObject)
                    characterSystem.CurrentCombinerState = CombinerState.Combined;
                _callback?.Invoke(returnSkinnedMeshes);

                _callback = null;

                DequeueFirst();
                EnqueueCharacterBake();
            }
        }

    }
}