//using BSS.Logging;
//using System;
//using System.Data;
//using System.Data.SQLite;
//using System.Net.Sockets;

//namespace Server
//{
//    internal static partial class Worker
//    {
//        private unsafe static void UpdatePassword(Socket connection, String header, ref readonly UserDB.User user)
//        {
//            if (!GetContent(header, connection, out String content)) return;

//            if (!ParseUserChangePassword(content, out UserChangePassword userConfiguration))
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

//            if (user.LoginUsername != userConfiguration.LoginUsername)
//            {
//                Log.FastLog($"'{user.LoginUsername}' attempted to change the configuration of user '{userConfiguration.LoginUsername}' -> sending 403", LogSeverity.Alert, "ChangePassword");
//                HTTP.ERRORS.Send_403(connection);
//                return;
//            }

//            if (!UserDB.UserExistsPlusEnabled(user.LoginUsername))
//            {
//                Log.FastLog($"'{user.LoginUsername}' was unable to update user '{userConfiguration.LoginUsername}', target not found", LogSeverity.Warning, "ChangePassword");
//                HTTP.ERRORS.Send_404(connection);
//            }

//            SQLiteCommand command = databaseConnection.CreateCommand();

//            Byte[] password;
//            Byte[] salt;

//            String encodedPassword;
//            String encodedSalt;

//            if (userConfiguration.DisplayUsername == null && userConfiguration.Password == null)
//            {
//                RedirectClient(connection, new(HTTP.ResponseType.HTTP_303, URL_ROOT));
//                return;
//            }
//            else if (userConfiguration.Password != null && userConfiguration.DisplayUsername == null)
//            {
//                (password, salt) = Authentication.CreateHash(userConfiguration.LoginUsername, userConfiguration.Password);
//                encodedPassword = Convert.ToBase64String(password);
//                encodedSalt = Convert.ToBase64String(salt);

//                command.CommandText = $"UPDATE User SET HashedPassword = @password, Salt = @salt WHERE LoginUsername = @loginUsername";
//                command.Parameters.Add("@loginUsername", DbType.String).Value = userConfiguration.LoginUsername;
//                command.Parameters.Add("@password", DbType.String).Value = encodedPassword;
//                command.Parameters.Add("@salt", DbType.String).Value = encodedSalt;
//            }
//            else if (userConfiguration.DisplayUsername != null && userConfiguration.Password == null)
//            {
//                command.CommandText = $"UPDATE User SET DisplayName = @displayName WHERE LoginUsername = @loginUsername";
//                command.Parameters.Add("@loginUsername", DbType.String).Value = userConfiguration.LoginUsername;
//                command.Parameters.Add("@displayName", DbType.String).Value = userConfiguration.DisplayUsername;
//            }
//            else // (userConfiguration.Password != null && userConfiguration.DisplayUsername != null) 
//            {
//                (password, salt) = Authentication.CreateHash(userConfiguration.LoginUsername!, userConfiguration.Password!);
//                encodedPassword = Convert.ToBase64String(password);
//                encodedSalt = Convert.ToBase64String(salt);

//                command.CommandText = $"UPDATE User SET DisplayName = @displayName, HashedPassword = @password, Salt = @salt WHERE LoginUsername = @loginUsername";
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
//                    Log.FastLog($"'{user.LoginUsername}' updated user '{userConfiguration.LoginUsername}' successfully", LogSeverity.Info, "ChangePassword");

//                    if (user.IsAdministrator) HTML.CGI.SendControlPanel(connection, in user, "<span style=\"color: green; font-weight: bold\">Updated user successfully</span>", true);
//                    else HTML.CGI.SendUserFilesView(connection, in user, in user, "<span style=\"color: green; font-weight: bold\">Updated user successfully</span>", true);
//                }
//                else
//                {
//                    Log.FastLog($"'{user.LoginUsername}': no database entries were updated for user '{userConfiguration.LoginUsername}'", LogSeverity.Info, "ChangePassword");
                    
//                    if (user.IsAdministrator) HTML.CGI.SendControlPanel(connection, in user, "<span style=\"color: orangered; font-weight: bold\">No database entries were updated</span>", true);
//                    else HTML.CGI.SendUserFilesView(connection, in user, in user, "<span style=\"color: orangered; font-weight: bold\">No database entries were updated</span>", true);
//                }
//            }
//            catch (Exception exception)
//            {
//                Log.FastLog($"An error occurred while '{user.LoginUsername}' attempted to updated user '{userConfiguration.LoginUsername}': " + exception.Message, LogSeverity.Error, "ChangePassword");
                
//                if (user.IsAdministrator) HTML.CGI.SendControlPanel(connection, in user, "<span style=\"color: orangered; font-weight: bold\">Error updating user</span>", true);
//                else HTML.CGI.SendUserFilesView(connection, in user, in user, "<span style=\"color: orangered; font-weight: bold\">Error updating user</span>", true);
//            }

//            command.Dispose();
//            databaseConnection.Close();
//            databaseConnection.Dispose();
//            return;
//        }
//    }
//}