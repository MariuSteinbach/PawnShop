using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace PawnShop.Models
{
    [Serializable]
    public class Config
    {
        public int Exports { get; set; }

        public async Task<bool> Load()
        {
            StorageFile ConfigFile = null;
            try
            {
                ConfigFile = await ApplicationData.Current.LocalFolder.GetFileAsync("local.config");
                if (ConfigFile != null)
                {
                    FileStream fs = new FileStream(ConfigFile.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    MemoryStream ms = new MemoryStream();
                    BinaryFormatter bf = new BinaryFormatter();
                    Byte[] encrypted = null;
                    try
                    {
                        encrypted = new Byte[fs.Length];
                        await fs.ReadAsync(encrypted, 0, Convert.ToInt32(fs.Length));
                    }
                    catch (SerializationException)
                    {
                        throw;
                    }
                    finally
                    {
                        fs.Close();
                    }
                    Byte[] decrypted = Crypter.Decrypt(encrypted);
                    ms.Write(decrypted, 0, decrypted.Length);
                    ms.Seek(0, SeekOrigin.Begin);
                    Config temp = (Config)bf.Deserialize(ms);
                    this.Exports = temp.Exports;
                    return true;
                }
            }
            catch
            {

            }
            return false;
        }
        public async void Save()
        {
            StorageFile ConfigFile = null;
            try
            {
                ConfigFile = await ApplicationData.Current.LocalFolder.CreateFileAsync($"local.config", CreationCollisionOption.ReplaceExisting);
            }
            catch (Exception)
            {
                throw;
            }
            if (ConfigFile != null)
            {
                //Stream sw = new Stream(storageFile.Path);
                //Saving the workbook
                FileStream fs = new FileStream(ConfigFile.Path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                MemoryStream ms = new MemoryStream();
                BinaryFormatter bf = new BinaryFormatter();
                try
                {
                    bf.Serialize(ms, this);
                    ms.Seek(0, SeekOrigin.Begin);
                    Byte[] bytes = new byte[ms.Length];
                    ms.Read(bytes, 0, (int)ms.Length);
                    Byte[] encrypted = Crypter.Encrypt(bytes);
                    await fs.WriteAsync(encrypted, 0, encrypted.Count());
                }
                catch (SerializationException)
                {
                    throw;
                }
                finally
                {
                    fs.Close();
                }
            }
        }
    }
}
