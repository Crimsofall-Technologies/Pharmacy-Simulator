using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

public static class DataSerializer
{
    static string path = Application.persistentDataPath + "/save.dat";

    public static void SavePlayer(PlayerData data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);
        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static PlayerData LoadData()
    {
        if(File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            PlayerData playerData = formatter.Deserialize(stream) as PlayerData;
            stream.Close(); 
            return playerData;
        }

        Debug.Log("No file exsists on path: " + path);
        return null;
    }

    public static void DeleteSaves()
    {
        if(File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
