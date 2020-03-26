﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AgileProject.DAO
{
    class UserDAO
    {
        /// <summary>
        ///  Get Connection String from Configuration
        /// </summary>
        private readonly string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["db"].ConnectionString;

        /// <summary>
        /// Login Authentication
        /// </summary>
        /// <param name="email">emaill address of the account</param>
        /// <param name="pw">password for this account</param>
        /// <returns>true/false of login successful</returns>
        public Boolean Authentication(String email, String pw)
        {
            Boolean authenticated = false;

            
            MySqlCommand comm = new MySqlCommand(" SELECT COUNT(*) FROM ACCOUTN WHERE EMAIL_ADDRESS = @EMAIL AND PASSWORD = SHA2(CONCAT(@PASSWORD, SALT), 256) ");
            comm.Parameters.AddWithValue("@EMAIL", email);
            comm.Parameters.AddWithValue("@PASSWORD", pw);

            Check(ref comm, ref authenticated);
            return authenticated;
        }

        /// <summary>
        /// Check is the email existed in the database
        /// </summary>
        /// <param name="email">email address of the account</param>
        /// <returns>true/false of existence of the email address</returns>
        public Boolean FindEmail(String email)
        {
            Boolean found = false;

            MySqlCommand comm = new MySqlCommand(" SELECT COUNT(*) FROM ACCOUNT EMAIL_ADDRESS = @EMAIL ");
            comm.Parameters.AddWithValue("@EMAIL", email);

            Check(ref comm, ref found);

            return found;
        }

        /// <summary>
        /// Add a new sign up user
        /// </summary>
        /// <param name="firstName">First name of the user</param>
        /// <param name="lastName">Last name of the user</param>
        /// <param name="email">emaill address of the user</param>
        /// <param name="password">password for the account</param>
        public void AddNewUser(String firstName, String lastName,  String email, String password)
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            MySqlCommand comm = new MySqlCommand(" INSERT INTO ACCOUNT (FIRST_NAME, LAST_NAME, EMAIL_ADDRESS, PASSWORD, SALT) "
                                               + " VALUES (@FNAME, @LNAME, @EMAIL, SHA2(CONCAT(@PASSWORD, @SALT)), @SALT) ", conn);
            comm.Parameters.AddWithValue("@FNAME", firstName);
            comm.Parameters.AddWithValue("@LNAME", lastName);
            comm.Parameters.AddWithValue("@EMAIL", email);
            comm.Parameters.AddWithValue("@PASSWORD", password);
            comm.Parameters.AddWithValue("@SALT", GenerateRandomSalt());

            try
            {
                conn.Open();
                comm.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            conn.Close();
        }

        /// <summary>
        /// Update the user's information
        /// </summary>
        /// <param name="key">User info key</param>
        /// <param name="str">User information that needs to be updated</param>
        public void UpdateUserInfo(UserInfoKey key, String str)
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            MySqlCommand comm = new MySqlCommand();
            comm.Connection = conn;
            String keyString = key.ToString();
            comm.CommandText = key != UserInfoKey.PASSWORD ?
                            "UPDATE ACCOUNT SET " + keyString + " = @" + keyString :
                            "UPDATE ACCOUNT SET " + keyString + " = SHA2(CONCAT(@" + keyString + ", SALT), SALT)";
            comm.Parameters.AddWithValue("@" + keyString, str);

            try
            {
                conn.Open();
                comm.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            conn.Close();
        }

        /// <summary>
        /// Generate a random salt
        /// </summary>
        /// <returns>salt string</returns>
        private String GenerateRandomSalt()
        {
            char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[4 * 256];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(256);
            for (int i = 0; i < 256; i++)
            {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % chars.Length;

                result.Append(chars[idx]);
            }

            return result.ToString();
        }


        /// <summary>
        /// Check is the data exist
        /// </summary>
        /// <param name="comm">A count() SQL statement </param>
        /// <param name="b">boolean variable to hold the value</param>
        private void Check(ref MySqlCommand comm, ref Boolean b)
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            comm.Connection = conn;
            try
            {
                conn.Open();
                MySqlDataReader reader = comm.ExecuteReader();

                while (reader.Read())
                {
                    b = reader.GetInt16(0) == 1 ? true : false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            conn.Close();
        }
    }

    enum UserInfoKey
    {
        FIRST_NAME = 0,
        LAST_NAME = 1,
        EMAIL_ADDRESS = 2,
        PASSWORD = 3
    }
}
