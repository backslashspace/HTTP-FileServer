//using BSS.Logging;
//using System;
//using System.Net.Sockets;
//using System.Text;
//using System.Data.SQLite;
//using System.Data;
//using System.IO;
//using System.Web;

//namespace Server
//{
//    internal static partial class Worker
//    {
//        internal unsafe static void CreateUser(Socket connection, String header, String[] pathParts, ref UserDB.User user)
//        {
//            if (!GetContent(header, connection, out String content)) return;

//            if (!ParseUserConfiguration(content, out UserConfiguration newUser))
//            {
//                HTTP.ERRORS.Send_400(connection);
//                return;
//            }

//            for (Int32 i = 0; i < newUser.LoginUsername.Length; ++i)
//            {
//                if (!Char.IsLetterOrDigit(newUser.LoginUsername[i]))
//                {
//                    Log.FastLog($"'{user.LoginUsername}' attempted to create a user with an invalid character '{newUser.LoginUsername[i]}'", LogSeverity.Warning, "CreateUser");
//                    HTML.CGI.SendControlPanel(connection, in user, $"<span style=\"color: orangered; font-weight: bold\">Invalid login name character: '{HttpUtility.HtmlEncode($"{newUser.LoginUsername[i]}")}'</span>", true);
//                    return;
//                }
//            }

//            if (newUser.LoginUsername.Length > 128)
//            {
//                Log.FastLog($"'{user.LoginUsername}' attempted to create a user with a name longer than 128 character -> sending 400", LogSeverity.Warning, "CreateUser");
//                HTTP.ERRORS.Send_400(connection);
//                return;
//            }

//            if (UserDB.UserExistsPlusEnabled(newUser.LoginUsername))
//            {
//                Log.FastLog($"'{user.LoginUsername}' attempted to create a new user that already exists", LogSeverity.Info, "CreateUser");
//                HTML.CGI.SendControlPanel(connection, in user, "<span style=\"color: orangered; font-weight: bold\">User already exists</span>", true);
//                return;
//            }

//            // # # # # # # # # # # # # # # #

//            if (!UserDB.GetDatabaseConnection(out SQLiteConnection databaseConnection))
//            {
//                HTTP.ERRORS.Send_500(connection);
//                return;
//            }

//            (Byte[] password, Byte[] salt) = Authentication.CreateHash(newUser.LoginUsername, newUser.Password);

//            String encodedPassword = Convert.ToBase64String(password);
//            String encodedSalt = Convert.ToBase64String(salt);

//            SQLiteCommand command = new($"INSERT INTO User (LoginUsername, DisplayName, HashedPassword, Salt, IsAdministrator, IsEnabled, Read, Write) VALUES (@loginUsername, @displayUsername, @encodedPassword, @encodedSalt, 0, {*(Byte*)&newUser.IsEnabled}, {*(Byte*)&newUser.Read}, {*(Byte*)&newUser.Write});", databaseConnection);
//            command.Parameters.Add("@loginUsername", DbType.String).Value = newUser.LoginUsername;
//            command.Parameters.Add("@displayUsername", DbType.String).Value = newUser.DisplayUsername;
//            command.Parameters.Add("@encodedPassword", DbType.String).Value = encodedPassword;
//            command.Parameters.Add("@encodedSalt", DbType.String).Value = encodedSalt;

//            if (command.ExecuteNonQuery() == 0)
//            {
//                Log.FastLog($"'{user.LoginUsername}' failed to created user '{newUser.LoginUsername}'", LogSeverity.Error, "CreateUser");
//                HTTP.ERRORS.Send_500(connection);
//                command.Dispose();
//                return;
//            }

//            command.Dispose();

//            Directory.CreateDirectory(Program.AssemblyPath + "files\\" + newUser.LoginUsername);

//            Log.FastLog($"'{user.LoginUsername}' successfully created user '{newUser.LoginUsername}'", LogSeverity.Info, "CreateUser");
//            HTML.CGI.SendControlPanel(connection, in user, "<span style=\"color: green; font-weight: bold\">User created successfully</span>", true);
//        }
//    }

//    internal static partial class HTML
//    {
//        internal static partial class CGI
//        {
//            internal static void SendCreateUserView(Socket connection)
//            {
//                Log.Debug("controlPanel\\createUser.html", "SendFile()");

//                String fileContent = Worker.ReadFileText("controlPanel\\createUser.html");

//                Byte[] buffer = Encoding.UTF8.GetBytes(fileContent);

//                HTTP.CraftHeader(new HTTP.HeaderOptions(HTTP.ResponseType.HTTP_200, new HTTP.ContentOptions(HTTP.ContentType.HTML), (UInt64)buffer.LongLength), out Byte[] headerBuffer);

//                connection.Send(headerBuffer, 0, headerBuffer.Length, SocketFlags.None);
//                connection.Send(buffer, 0, buffer.Length, SocketFlags.None);

//                Worker.CloseConnection(connection);
//            }
//        }
//    }
//}