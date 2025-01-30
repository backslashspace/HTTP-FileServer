using System;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Text;
using BSS.Logging;
using BSS.Threading;
using System.IO;
using System.Web;

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class CGI
        {
            internal static void SendUserFilesView(Socket connection, ref readonly UserDB.User invokingUser, ref readonly UserDB.User destinationUser, String insertInfoString = null!, Boolean setSelfURL = false)
            {
                Log.Debug("userFiles.html", "SendFile()");
                String fileContent = Worker.ReadFileText("userFiles.html");

                //

                if (invokingUser.Write || invokingUser.IsAdministrator)
                {
                    fileContent = Regex.Replace(fileContent, "<!-- #UPLOAD#ANCHOR# -->", "<form action=\"/fileSharing/files/upload\" method=\"get\" style=\"display: inline;\">\r\n    <button type=\"submit\">Upload File</button>\r\n</form>");
                }

                if (invokingUser.Read || invokingUser.IsAdministrator)
                {
                    if (!InsertFiles(connection, destinationUser.LoginUsername, ref fileContent)) return;
                }

                if (invokingUser.IsAdministrator) fileContent = Regex.Replace(fileContent, "<!-- #THREADPOOL#ANCHOR# -->", $"Thread pool Threads: {ThreadPoolFast.Count}/{ThreadPoolFast.Capacity}");

                if (destinationUser.LoginUsername == invokingUser.LoginUsername) fileContent = Regex.Replace(fileContent, "<!-- #HEADLINE#ANCHOR# -->", "Your Files");
                else fileContent = Regex.Replace(fileContent, "<!-- #HEADLINE#ANCHOR# -->", $"{HttpUtility.HtmlEncode(destinationUser.DisplayName)}'s Files");

                if (insertInfoString != null) fileContent = Regex.Replace(fileContent, "<!-- #INFO#ANCHOR# -->", insertInfoString);

                if (setSelfURL) fileContent = Regex.Replace(fileContent, "<!-- #SCRIPT#ANCHOR# -->", "<script type=\"text/javascript\">\r\n\t\t\twindow.history.replaceState(null, document.title, \"/fileSharing/controlPanel\")\r\n\t\t</script>");

                fileContent = Regex.Replace(fileContent, "<!-- #DISPLAY#USERNAME#ANCHOR# -->", invokingUser.DisplayName);

                //

                Byte[] buffer = Encoding.UTF8.GetBytes(fileContent);

                HTTP.ContentOptions contentOptions = new(HTTP.ContentType.HTML);
                HTTP.HeaderOptions headerOptions = new(HTTP.ResponseType.HTTP_200, contentOptions, (UInt64)buffer.LongLength);
                HTTP.CraftHeader(headerOptions, out Byte[] headerBuffer);

                connection.Send(headerBuffer, 0, headerBuffer.Length, SocketFlags.None);
                connection.Send(buffer, 0, buffer.Length, SocketFlags.None);

                Log.FastLog($"'{invokingUser.LoginUsername}' enumerated files for '{destinationUser.LoginUsername}'", LogSeverity.Info, "FileInfo");

                Worker.CloseConnection(connection);
            }

            //

            private static Boolean InsertFiles(Socket connection, String loginUsername, ref String fileContent)
            {
                if (loginUsername.Length > 128)
                {
                    Log.FastLog($"Detected long username (over 128 char), skipping: {loginUsername}", LogSeverity.Warning, "FileInfo");
                    fileContent = Regex.Replace(fileContent, "<!-- #FILE#ANCHOR# -->", "<td><span style=\"margin: 1em\">error - user name to long (more than 128 chars) - skipping file system check for compatibility reasons</span></td>");
                    return true;
                }

                if (!Directory.Exists("\\\\?\\" + Worker.AssemblyPath + "\\files\\" + loginUsername))
                {
                    fileContent = Regex.Replace(fileContent, "<!-- #FILE#ANCHOR# -->", "<td><span style=\"margin: 1em\">No files found!</span></td>");
                    return true;
                }

                try
                {
                    DirectoryInfo directoryInfo = new("\\\\?\\" + Worker.AssemblyPath + "\\files\\" + loginUsername);
                    FileInfo[] fileInfo = directoryInfo.GetFiles("*", SearchOption.TopDirectoryOnly);

                    if (fileInfo.Length == 0)
                    {
                        fileContent = Regex.Replace(fileContent, "<!-- #FILE#ANCHOR# -->", "<td><span style=\"margin: 1em\">No files found!</span></td>");
                        return true;
                    }

                    for (Int32 i = 0; i < fileInfo.Length; ++i)
                    {
                        fileContent = Regex.Replace(fileContent, "<!-- #FILE#ANCHOR# -->", $@"
<tr>
    <td><span style=""margin: 1em"">{HttpUtility.HtmlEncode(fileInfo[i].Name)}</span></td>
    <td>
        <form method=""POST"" action=""/fileSharing/files/remove"" style=""width: 64px; margin-top: 10px; margin-bottom: -2px; margin-left: 2px; margin-right: 2px;"">
            <input type=""hidden"" name=""user"" value=""{HttpUtility.HtmlEncode(loginUsername)}"" required readonly />
            <input type=""hidden"" name=""name"" value=""{HttpUtility.HtmlEncode(fileInfo[i].Name)}"" required readonly />
            <input type=""submit"" value=""Delete"" style=""height:23px; font-size: 13px"" />
        </form>
    </td>
    <td>
        <form method=""POST"" action=""/fileSharing/files/download"" style=""width: 84px; margin-top: 10px; margin-bottom: -2px; margin-left: 2px; margin-right: 2px;"">
            <input type=""hidden"" name=""user"" value=""{HttpUtility.HtmlEncode(loginUsername)}"" required readonly />
            <input type=""hidden"" name=""name"" value=""{HttpUtility.HtmlEncode(fileInfo[i].Name)}"" required readonly />
            <input type=""submit"" value=""Download"" style=""height:23px; font-size: 13px"" />
        </form>
    </td>
    <td><span style=""margin: 1em"">{GetSizeString((UInt64)fileInfo[i].Length)}</span></td>
</tr>

<!-- #FILE#ANCHOR# -->

");
                    }
                }
                catch (Exception exception)
                {
                    Log.FastLog($"An error occurred whilst creating the file table for user '{loginUsername}': " + exception.Message, LogSeverity.Error, "FileInfo");
                    HTTP.ERRORS.Send_500(connection);
                    return false;
                }

                return true;
            }

            //

            internal static String GetSizeString(UInt64 sizeInBytes)
            {
                // HowsThis4Size
                if (sizeInBytes > 1125899906842624) return $"{Math.Round((Double)sizeInBytes / (Double)1125899906842624, 2, MidpointRounding.AwayFromZero)} PiB";
    
                if (sizeInBytes > 1099511627776) return $"{Math.Round((Double)sizeInBytes / (Double)1099511627776, 2, MidpointRounding.AwayFromZero)} TiB";
 
                if (sizeInBytes > 1073741824) return $"{Math.Round((Double)sizeInBytes / (Double)1073741824, 2, MidpointRounding.AwayFromZero)} GiB";

                if (sizeInBytes > 1048576) return $"{Math.Round((Double)sizeInBytes / (Double)1048576, 2, MidpointRounding.AwayFromZero)} MiB";

                if (sizeInBytes > 1024) return $"{Math.Round((Double)sizeInBytes / (Double)1024, 2,MidpointRounding.AwayFromZero)} KiB";

                return $"{sizeInBytes} B";
            }
        }
    }
}