using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace RailAppWPF.Classes
{
    public class mySecurity
    {

        public static string directorypath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\RailApp";

        public static string fileName = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\RailApp\\settings.json";

        public static byte[] s_additionalEntropy = { 3, 5, 4, 8, 5, 2 };

        public static mySettings_decrypted Pull_Settings()
        {

            if (File.Exists(fileName))
            {

                using FileStream openStream = File.OpenRead(fileName);
                mySettings_encrypted app_settings_encrypted = JsonSerializer.Deserialize<mySettings_encrypted>(openStream);

                mySettings_decrypted app_settings_decrypted = new mySettings_decrypted
                {

                    User_name = app_settings_encrypted.User_name,
                    Stored_api_username = encrypted_byte_arr_to_string(app_settings_encrypted.Stored_api_username),
                    Stored_api_password = encrypted_byte_arr_to_string(app_settings_encrypted.Stored_api_password),
                    log_path = app_settings_encrypted.log_path,
                    export_path = app_settings_encrypted.export_path
                };

                return app_settings_decrypted;
            }
            else
            {

                Directory.CreateDirectory(directorypath);
                mySettings_decrypted app_settings_decrypted = new();
                return app_settings_decrypted;

            }


        }

        public static void Save_Settings(mySettings_decrypted app_settings)
        {

            mySettings_encrypted app_settings_encrypted = new mySettings_encrypted
            {

                User_name = app_settings.User_name,
                Stored_api_username = string_to_encrypted_byte_arr(app_settings.Stored_api_username),
                Stored_api_password = string_to_encrypted_byte_arr(app_settings.Stored_api_password),
                log_path = app_settings.log_path,
                export_path = app_settings.export_path

            };

            JsonSerializerOptions options = new JsonSerializerOptions
            {

                WriteIndented = true,

            };

            string jsonString = JsonSerializer.Serialize(app_settings_encrypted, options);
            File.WriteAllText(fileName, jsonString);

        }

        public static byte[] string_to_encrypted_byte_arr(string str)
        {

            byte[] bytes = Encoding.UTF8.GetBytes(str);

            byte[] bytes_encrypted = ProtectedData.Protect(bytes, s_additionalEntropy, DataProtectionScope.CurrentUser);

            return bytes_encrypted;

        }

        public static string encrypted_byte_arr_to_string(byte[] bytes)
        {

            byte[] bytes_decrypted = ProtectedData.Unprotect(bytes, s_additionalEntropy, DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(bytes_decrypted);

        }

    }

    public class mySettings_encrypted
    {

        public string User_name { get; set; }
        public byte[] Stored_api_username { get; set; }
        public byte[] Stored_api_password { get; set; }
        public string log_path { get; set; }
        public string export_path { get; set; }

    }

    public class mySettings_decrypted

    {
        public string User_name { get; set; }
        public string Stored_api_username { get; set; }
        public string Stored_api_password { get; set; }
        public string log_path { get; set; }
        public string export_path { get; set; }

    }

}
