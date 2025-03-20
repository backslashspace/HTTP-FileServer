using BSS.Logging;
using System;
using System.Data.SQLite;
using System.Data;
using System.IO;
using System.Web;

namespace Server
{
    internal static partial class Worker
    {
        internal unsafe static void CreateUser(SecureSocket connection, String header, String[] pathParts, ref readonly UserDB.User user)
        {
            if (!HTTP.GetContentLength(header, out Int64 contentLength))
            {
                HTTP.ERRORS.Send_400(connection);
                return;
            }

            if (contentLength > 4096)
            {
                Log.FastLog(user.LoginUsername + " send a content body that was over 4096 bytes while attempting to create a new user", LogSeverity.Warning, "CreateUser");
                HTTP.ERRORS.Send_400(connection);
                return;
            }

            Span<Byte> buffer = stackalloc Byte[(Int32)contentLength];
            connection.SslStream!.ReadExactly(buffer);

            if (buffer.Length < 32)
            {
                HTTP.ERRORS.Send_400(connection);
                return;
            }

            if (!ParseUserConfiguration(ref buffer, out UserConfiguration newUser))
            {
                HTTP.ERRORS.Send_400(connection);
                return;
            }

            if (newUser.LoginUsername == null || !Tools.IsNumberOrLetterOrDashOrUnderscore(newUser.LoginUsername))
            {
                SendControlPanel(connection, in user, $"<span style=\"color: orangered; font-weight: bold\">Invalid login name: only [a-zA-Z0-9_-] is allowed</span>", true);
            }

            if (newUser.DisplayUsername == null || !Tools.IsNumberOrLetterOrDashOrUnderscore(newUser.DisplayUsername))
            {
                //
            }

            if (newUser.Password == null || newUser.Password.Length < 12)
            {
                //
            }

            newUser.IsEnabled = *(Int32*)&newUser.IsEnabled == 1; 
            newUser.Write = *(Int32*)&newUser.Write == 1; 
            newUser.Read = *(Int32*)&newUser.Read == 1; 

            HTTP.ERRORS.Send_501(connection);

            //if (!ParseUserConfiguration(content, out UserConfiguration newUser))
            //{
            //    HTTP.ERRORS.Send_400(connection);
            //    return;
            //}

            //for (Int32 i = 0; i < newUser.LoginUsername.Length; ++i)
            //{
            //    if (!Char.IsLetterOrDigit(newUser.LoginUsername[i]))
            //    {
            //        Log.FastLog($"'{user.LoginUsername}' attempted to create a user with an invalid character '{newUser.LoginUsername[i]}'", LogSeverity.Warning, "CreateUser");
            //        HTML.CGI.SendControlPanel(connection, in user, $"<span style=\"color: orangered; font-weight: bold\">Invalid login name character: '{HttpUtility.HtmlEncode($"{newUser.LoginUsername[i]}")}'</span>", true);
            //        return;
            //    }
            //}

            //if (newUser.LoginUsername.Length > 128)
            //{
            //    Log.FastLog($"'{user.LoginUsername}' attempted to create a user with a name longer than 128 character -> sending 400", LogSeverity.Warning, "CreateUser");
            //    HTTP.ERRORS.Send_400(connection);
            //    return;
            //}

            //if (UserDB.UserExistsPlusEnabled(newUser.LoginUsername))
            //{
            //    Log.FastLog($"'{user.LoginUsername}' attempted to create a new user that already exists", LogSeverity.Info, "CreateUser");
            //    HTML.CGI.SendControlPanel(connection, in user, "<span style=\"color: orangered; font-weight: bold\">User already exists</span>", true);
            //    return;
            //}

            //// # # # # # # # # # # # # # # #

            //if (!UserDB.GetDatabaseConnection(out SQLiteConnection databaseConnection))
            //{
            //    HTTP.ERRORS.Send_500(connection);
            //    return;
            //}

            //(Byte[] password, Byte[] salt) = Authentication.CreateHash(newUser.LoginUsername, newUser.Password);

            //String encodedPassword = Convert.ToBase64String(password);
            //String encodedSalt = Convert.ToBase64String(salt);

            //SQLiteCommand command = new($"INSERT INTO User (LoginUsername, DisplayName, HashedPassword, Salt, IsAdministrator, IsEnabled, Read, Write) VALUES (@loginUsername, @displayUsername, @encodedPassword, @encodedSalt, 0, {*(Byte*)&newUser.IsEnabled}, {*(Byte*)&newUser.Read}, {*(Byte*)&newUser.Write});", databaseConnection);
            //command.Parameters.Add("@loginUsername", DbType.String).Value = newUser.LoginUsername;
            //command.Parameters.Add("@displayUsername", DbType.String).Value = newUser.DisplayUsername;
            //command.Parameters.Add("@encodedPassword", DbType.String).Value = encodedPassword;
            //command.Parameters.Add("@encodedSalt", DbType.String).Value = encodedSalt;

            //if (command.ExecuteNonQuery() == 0)
            //{
            //    Log.FastLog($"'{user.LoginUsername}' failed to created user '{newUser.LoginUsername}'", LogSeverity.Error, "CreateUser");
            //    HTTP.ERRORS.Send_500(connection);
            //    command.Dispose();
            //    return;
            //}

            //command.Dispose();

            //Directory.CreateDirectory(Program.AssemblyPath + "files\\" + newUser.LoginUsername);

            //Log.FastLog($"'{user.LoginUsername}' successfully created user '{newUser.LoginUsername}'", LogSeverity.Info, "CreateUser");
            //HTML.CGI.SendControlPanel(connection, in user, "<span style=\"color: green; font-weight: bold\">User created successfully</span>", true);
        }
    }


}