using BSS.Logging;
using System;
using System.Data;
using System.Data.SQLite;
using System.Net.Sockets;

namespace Server
{
    internal static partial class Worker
    {
        private unsafe static void UpdateUser(Socket connection, String header, ref readonly UserDB.User user)
        {
            if (!GetContent(header, connection, out String content)) return;

            if (!ParseUserConfiguration(content, out UserConfiguration userConfiguration, true))
            {
                HTTP.ERRORS.Send_400(connection);
                return;
            }

            if (!UserDB.GetDatabaseConnection(out SQLiteConnection databaseConnection))
            {
                HTTP.ERRORS.Send_500(connection);
                return;
            }

            //

            SQLiteCommand command = databaseConnection.CreateCommand();

            Byte[] password;
            Byte[] salt;

            String encodedPassword;
            String encodedSalt;

            if (userConfiguration.DisplayUsername == null && userConfiguration.Password == null)
            {
                command.CommandText = $"UPDATE User SET IsEnabled = {*(Byte*)&userConfiguration.IsEnabled}, Read = {*(Byte*)&userConfiguration.Read}, Write = {*(Byte*)&userConfiguration.Write} WHERE LoginUsername = @loginUsername";
                command.Parameters.Add("@loginUsername", DbType.String).Value = userConfiguration.LoginUsername;
            }
            else if (userConfiguration.Password != null && userConfiguration.DisplayUsername == null)
            {
                (password, salt) = Authentication.CreateHash(userConfiguration.LoginUsername, userConfiguration.Password);
                encodedPassword = Convert.ToBase64String(password);
                encodedSalt = Convert.ToBase64String(salt);

                command.CommandText = $"UPDATE User SET HashedPassword = @password, Salt = @salt, IsEnabled = {*(Byte*)&userConfiguration.IsEnabled}, Read = {*(Byte*)&userConfiguration.Read}, Write = {*(Byte*)&userConfiguration.Write} WHERE LoginUsername = @loginUsername";
                command.Parameters.Add("@loginUsername", DbType.String).Value = userConfiguration.LoginUsername;
                command.Parameters.Add("@password", DbType.String).Value = encodedPassword;
                command.Parameters.Add("@salt", DbType.String).Value = encodedSalt;
            }
            else if (userConfiguration.DisplayUsername != null && userConfiguration.Password == null)
            {
                command.CommandText = $"UPDATE User SET DisplayName = @displayName, IsEnabled = {*(Byte*)&userConfiguration.IsEnabled}, Read = {*(Byte*)&userConfiguration.Read}, Write = {*(Byte*)&userConfiguration.Write} WHERE LoginUsername = @loginUsername";
                command.Parameters.Add("@loginUsername", DbType.String).Value = userConfiguration.LoginUsername;
                command.Parameters.Add("@displayName", DbType.String).Value = userConfiguration.DisplayUsername;
            }
            else // (userConfiguration.Password != null && userConfiguration.DisplayUsername != null) 
            {
                (password, salt) = Authentication.CreateHash(userConfiguration.LoginUsername!, userConfiguration.Password!);
                encodedPassword = Convert.ToBase64String(password);
                encodedSalt = Convert.ToBase64String(salt);

                command.CommandText = $"UPDATE User SET DisplayName = @displayName, HashedPassword = @password, Salt = @salt, IsEnabled = {*(Byte*)&userConfiguration.IsEnabled}, Read = {*(Byte*)&userConfiguration.Read}, Write = {*(Byte*)&userConfiguration.Write} WHERE LoginUsername = @loginUsername";
                command.Parameters.Add("@loginUsername", DbType.String).Value = userConfiguration.LoginUsername;
                command.Parameters.Add("@displayName", DbType.String).Value = userConfiguration.DisplayUsername;
                command.Parameters.Add("@password", DbType.String).Value = encodedPassword;
                command.Parameters.Add("@salt", DbType.String).Value = encodedSalt;
            }

            //
            
            if (command.ExecuteNonQuery(CommandBehavior.SingleResult) == 0)
            {
                Log.FastLog($"'{user.LoginUsername}': no database entries were updated for user '{userConfiguration.LoginUsername}'", LogSeverity.Info, "UpdateUser");
                HTML.CGI.SendControlPanel(connection, in user, "<span style=\"color: orangered; font-weight: bold\">No database entries were updated</span>", true);
            }
            else
            {
                Log.FastLog($"'{user.LoginUsername}'updated user '{userConfiguration.LoginUsername}' successfully", LogSeverity.Info, "UpdateUser");
                HTML.CGI.SendControlPanel(connection, in user, "<span style=\"color: green; font-weight: bold\">Updated user successfully</span>", true);
            }

            command.Dispose();
        }
    }
}