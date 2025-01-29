﻿using BSS.Logging;
using BSS.Threading;
using System;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Text;
using System.Data.SQLite;
using System.Data;
using System.Web;
using System.IO;

namespace Server
{
    internal static partial class Worker
    {
        internal unsafe static void CreateUser(Socket connection, String header, String[] pathParts, ref UserDB.User user)
        {
            if (!GetContent(header, connection, out String content)) return;

            if (!ParseUserConfiguration(content, out UserConfiguration userConfiguration))
            {
                HTTP.ERRORS.Send_400(connection);
                return;
            }

            if (userConfiguration.LoginUsername.Length > 128)
            {
                Log.FastLog($"'{user.LoginUsername}' attempted to create a user with a name longer than 128 character -> sending 400", LogSeverity.Warning, "CreateUser");
                HTTP.ERRORS.Send_400(connection);
                return;
            }

            if (UserDB.UserExistsPlusEnabled(userConfiguration.LoginUsername))
            {
                Log.FastLog($"'{user.LoginUsername}' attempted to create a new user that already exists", LogSeverity.Info, "CreateUser");
                HTML.CGI.SendControlPanel(connection, in user, "<span style=\"color: orangered; font-weight: bold\">User already exists</span>", true);
                return;
            }

            // # # # # # # # # # # # # # # #

            if (!UserDB.GetDatabaseConnection(out SQLiteConnection databaseConnection))
            {
                HTTP.ERRORS.Send_500(connection);
                return;
            }

            (Byte[] password, Byte[] salt) = Authentication.CreateHash(userConfiguration.LoginUsername, userConfiguration.Password);

            String encodedPassword = Convert.ToBase64String(password);
            String encodedSalt = Convert.ToBase64String(salt);

            SQLiteCommand command = new($"INSERT INTO User (LoginUsername, DisplayName, HashedPassword, Salt, IsAdministrator, IsEnabled, Read, Write) VALUES (@loginUsername, @displayUsername, @encodedPassword, @encodedSalt, 0, {*(Byte*)&userConfiguration.IsEnabled}, {*(Byte*)&userConfiguration.Read}, {*(Byte*)&userConfiguration.Write});", databaseConnection);
            command.Parameters.Add("@loginUsername", DbType.String).Value = userConfiguration.LoginUsername;
            command.Parameters.Add("@displayUsername", DbType.String).Value = userConfiguration.DisplayUsername;
            command.Parameters.Add("@encodedPassword", DbType.String).Value = encodedPassword;
            command.Parameters.Add("@encodedSalt", DbType.String).Value = encodedSalt;

            if (command.ExecuteNonQuery() == 0)
            {
                Log.FastLog($"'{user.LoginUsername}' failed to created user '{userConfiguration.LoginUsername}'", LogSeverity.Error, "CreateUser");
                HTTP.ERRORS.Send_500(connection);
                command.Dispose();
                return;
            }

            command.Dispose();

            Directory.CreateDirectory(AssemblyPath + "\\files\\" + userConfiguration.LoginUsername);

            Log.FastLog($"'{user.LoginUsername}' successfully created user '{userConfiguration.LoginUsername}'", LogSeverity.Info, "CreateUser");
            HTML.CGI.SendControlPanel(connection, in user, "<span style=\"color: green; font-weight: bold\">User created successfully</span>", true);
        }
    }

    internal static partial class HTML
    {
        internal static partial class CGI
        {
            internal static void SendCreateUserView(Socket connection)
            {
                Log.Debug("controlPanel\\createUser.html", "SendFile()");

                String fileContent = Worker.ReadFileText("controlPanel\\createUser.html");

                Byte[] buffer = Encoding.UTF8.GetBytes(fileContent);

                HTTP.CraftHeader(new HTTP.HeaderOptions(HTTP.ResponseType.HTTP_200, new HTTP.ContentOptions(HTTP.ContentType.HTML), (UInt64)buffer.LongLength), out Byte[] headerBuffer);

                connection.Send(headerBuffer, 0, headerBuffer.Length, SocketFlags.None);
                connection.Send(buffer, 0, buffer.Length, SocketFlags.None);

                Worker.CloseConnection(connection);
            }
        }
    }
}