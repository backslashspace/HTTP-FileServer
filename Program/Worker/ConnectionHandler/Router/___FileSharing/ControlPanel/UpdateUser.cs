//using BSS.Logging;
//using System;
//using System.Data;
//using System.Data.SQLite;
//using System.Net.Sockets;

//namespace Server
//{
//    internal static partial class Worker
//    {
//        private unsafe static void UpdateUser(Socket connection, String header, ref readonly UserDB.User user)
//        {
//            if (!GetContent(header, connection, out String content)) return;

//            if (!ParseUserUpdate(content, out UserUpdate userConfiguration))
//            {
//                HTTP.ERRORS.Send_400(connection);
//                return;
//            }

//            if (!UserDB.GetDatabaseConnection(out SQLiteConnection databaseConnection))
//            {
//                HTTP.ERRORS.Send_500(connection);
//                return;
//            }

//            //

//            if (!UserIsValid(databaseConnection, userConfiguration.LoginUsername))
//            {
//                Log.FastLog($"'{user.LoginUsername}' was unable to update user '{userConfiguration.LoginUsername}', target not found", LogSeverity.Warning, "UpdateUser");
//                HTML.CGI.SendControlPanel(connection, in user, "<span style=\"color: orangered; font-weight: bold\">User not found</span>", true);
//                return;
//            }

//            //

//            SQLiteCommand command = databaseConnection.CreateCommand();

//            Byte[] password;
//            Byte[] salt;

//            String encodedPassword;
//            String encodedSalt;

//            if (userConfiguration.Remove)
//            {
//                command.CommandText = $"DELETE FROM User WHERE LoginUsername = @loginUsername";
//                command.Parameters.Add("@loginUsername", DbType.String).Value = userConfiguration.LoginUsername;

//                try
//                {
//                    if (command.ExecuteNonQuery() == 1)
//                    {
//                        Log.FastLog($"'{user.LoginUsername}' REMOVED user '{userConfiguration.LoginUsername}' successfully", LogSeverity.Info, "UpdateUser");
//                        HTML.CGI.SendControlPanel(connection, in user, "<span style=\"color: green; font-weight: bold\">Removed user successfully</span>", true);
//                    }
//                    else
//                    {
//                        Log.FastLog($"'{user.LoginUsername}' attempted to REMOVED user '{userConfiguration.LoginUsername}', but no rows were affected", LogSeverity.Warning, "UpdateUser");
//                        HTML.CGI.SendControlPanel(connection, in user, null!, true);
//                    }
//                }
//                catch (Exception exception)
//                {
//                    Log.FastLog($"An error occurred while '{user.LoginUsername}' attempted to remove user '{userConfiguration.LoginUsername}': " + exception.Message, LogSeverity.Error, "UpdateUser");
//                    HTML.CGI.SendControlPanel(connection, in user, "<span style=\"color: orangered; font-weight: bold\">Error removing user</span>", true);
//                }

//                command.Dispose();
//                databaseConnection.Close();
//                databaseConnection.Dispose();
//                return;
//            }

//            if (userConfiguration.DisplayUsername == null && userConfiguration.Password == null)
//            {
//                command.CommandText = $"UPDATE User SET IsEnabled = {*(Byte*)&userConfiguration.IsEnabled}, Read = {*(Byte*)&userConfiguration.Read}, Write = {*(Byte*)&userConfiguration.Write} WHERE LoginUsername = @loginUsername";
//                command.Parameters.Add("@loginUsername", DbType.String).Value = userConfiguration.LoginUsername;
//            }
//            else if (userConfiguration.Password != null && userConfiguration.DisplayUsername == null)
//            {
//                (password, salt) = Authentication.CreateHash(userConfiguration.LoginUsername, userConfiguration.Password);
//                encodedPassword = Convert.ToBase64String(password);
//                encodedSalt = Convert.ToBase64String(salt);

//                command.CommandText = $"UPDATE User SET HashedPassword = @password, Salt = @salt, IsEnabled = {*(Byte*)&userConfiguration.IsEnabled}, Read = {*(Byte*)&userConfiguration.Read}, Write = {*(Byte*)&userConfiguration.Write} WHERE LoginUsername = @loginUsername";
//                command.Parameters.Add("@loginUsername", DbType.String).Value = userConfiguration.LoginUsername;
//                command.Parameters.Add("@password", DbType.String).Value = encodedPassword;
//                command.Parameters.Add("@salt", DbType.String).Value = encodedSalt;
//            }
//            else if (userConfiguration.DisplayUsername != null && userConfiguration.Password == null)
//            {
//                command.CommandText = $"UPDATE User SET DisplayName = @displayName, IsEnabled = {*(Byte*)&userConfiguration.IsEnabled}, Read = {*(Byte*)&userConfiguration.Read}, Write = {*(Byte*)&userConfiguration.Write} WHERE LoginUsername = @loginUsername";
//                command.Parameters.Add("@loginUsername", DbType.String).Value = userConfiguration.LoginUsername;
//                command.Parameters.Add("@displayName", DbType.String).Value = userConfiguration.DisplayUsername;
//            }
//            else // (userConfiguration.Password != null && userConfiguration.DisplayUsername != null) 
//            {
//                (password, salt) = Authentication.CreateHash(userConfiguration.LoginUsername!, userConfiguration.Password!);
//                encodedPassword = Convert.ToBase64String(password);
//                encodedSalt = Convert.ToBase64String(salt);

//                command.CommandText = $"UPDATE User SET DisplayName = @displayName, HashedPassword = @password, Salt = @salt, IsEnabled = {*(Byte*)&userConfiguration.IsEnabled}, Read = {*(Byte*)&userConfiguration.Read}, Write = {*(Byte*)&userConfiguration.Write} WHERE LoginUsername = @loginUsername";
//                command.Parameters.Add("@loginUsername", DbType.String).Value = userConfiguration.LoginUsername;
//                command.Parameters.Add("@displayName", DbType.String).Value = userConfiguration.DisplayUsername;
//                command.Parameters.Add("@password", DbType.String).Value = encodedPassword;
//                command.Parameters.Add("@salt", DbType.String).Value = encodedSalt;
//            }

//            //

//            try
//            {
//                if (command.ExecuteNonQuery() == 1)
//                {
//                    Log.FastLog($"'{user.LoginUsername}' updated user '{userConfiguration.LoginUsername}' successfully", LogSeverity.Info, "UpdateUser");
//                    HTML.CGI.SendControlPanel(connection, in user, "<span style=\"color: green; font-weight: bold\">Updated user successfully</span>", true);
//                }
//                else
//                {
//                    Log.FastLog($"'{user.LoginUsername}': no database entries were updated for user '{userConfiguration.LoginUsername}'", LogSeverity.Info, "UpdateUser");
//                    HTML.CGI.SendControlPanel(connection, in user, "<span style=\"color: orangered; font-weight: bold\">No database entries were updated</span>", true);
//                }
//            }
//            catch (Exception exception)
//            {
//                Log.FastLog($"An error occurred while '{user.LoginUsername}' attempted to updated user '{userConfiguration.LoginUsername}': " + exception.Message, LogSeverity.Error, "UpdateUser");
//                HTML.CGI.SendControlPanel(connection, in user, "<span style=\"color: orangered; font-weight: bold\">Error updating user</span>", true);
//            }

//            command.Dispose();
//            databaseConnection.Close();
//            databaseConnection.Dispose();
//            return;
//        }

//        private static Boolean UserIsValid(SQLiteConnection databaseConnection, String loginUsername)
//        {
//            SQLiteCommand command = databaseConnection.CreateCommand();

//            command.CommandText = $"SELECT LoginUsername FROM User WHERE LoginUsername = @loginUsername";
//            command.Parameters.Add("@loginUsername", DbType.String).Value = loginUsername;

//            SQLiteDataReader dataReader = command.ExecuteReader(CommandBehavior.SingleRow);

//            Boolean test = dataReader.HasRows;

//            dataReader.Close();
//            command.Dispose();

//            return test; 
//        }
//    }
//}