using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AutoSiem;
using AutoSiem.Areas.Identity.Data;
using System.Text;
using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.Http.Extensions;

namespace test_auth.Controllers
{
    public class ScriptController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string SCRIPT_DIRECTORY = "Templates/Scripts/";
        private const string CONFIG_DIRECTORY = "Templates/Configurations/";

        public ScriptController(ApplicationDbContext context)
        {
            _context = context;
        }

        // this function is invoked when the user tries to download a script 
        // function is used to decode from base64 the template script file and tailor that with parameters our platform
        // then it sends it to the folder /downloads/<guid-of-platform>/
        // waiting to be zipped by the caller function and downloaded by the client 
        private void SetupScriptFile(Platform platform, string folderPath)
        {
            // read from predefined template of the current operating system of the platform
            string fileName = $"{((int)platform.Settings.OperatingSystem).ToString()}.script";
            string b64Encoded = System.IO.File.ReadAllText(SCRIPT_DIRECTORY + fileName);
            string content =  Encoding.UTF8.GetString(Convert.FromBase64String(b64Encoded));

            // populate our script with dynamic data here 
            // basically replacing on the variables defined in the file with data
            content = content.Replace("#DOMAIN#", Request.Host.ToString()); // put the domain of this server in the curl URL
            content = content.Replace("#PLATFORM_ID#", platform.Id.ToString()); // put the platform_id in the curl so the server knows which platform to populate the nodes

            // determine script extension
            string xtension = "sh";
            switch (platform.Settings.OperatingSystem)
            {
                // enum below Arch is unix based
                case OS n when n <= OS.Arch: // check enum for more information
                    xtension = "sh";
                    break;

                // above that is windows and unknown
                case OS n when n > OS.Arch && n != OS.Unknown: // WINDOWS Extension
                    xtension = "ps1";
                    break;

                default:
                    xtension = "unknown";
                    break;
            }

            // write file 
            // this is not a unix based file!
            // need to chmod 600 to use it 
            // TODO: use streamwriter to make it unix or windows based on the xtension above
            System.IO.File.WriteAllText($"{folderPath}/install.{xtension}", content);
        }

        void SetupLoggingConfigurationFile(Platform platform, string folderPath)
        {
            // if the operating is unix based
            // we use rsyslog as a logging service
            if(platform.Settings.OperatingSystem <= OS.Arch)
            {
                // read the template of rsyslog configuration
                // configuration is stored in the main directory of this project

                string rsyslog_config = System.IO.File.ReadAllText(CONFIG_DIRECTORY + "10-default.conf");
                rsyslog_config = rsyslog_config.Replace("#siem_ip#", platform.Siem.IpAddress);
                rsyslog_config = rsyslog_config.Replace("#siem_port#", platform.Siem.Port.ToString());

                // add the custom logging files to the configuration file
                string customPaths = "";
                foreach (string path in platform.Settings.CustomLogPaths.Split(":"))
                {
                    if(path == "") // if path is empty don't add
                        continue;

                    customPaths += "*.*\t\t\t\t\t" + path + "\n";
                }
                
                // replace the template variable with the above customPaths strings
                // this will allow all the files in custom paths to be monitored by rsyslog
                rsyslog_config = rsyslog_config.Replace("#custom_paths#", customPaths);

                // write to download file; awaiting to be zipped
                // stored in respective folder; folder is in the name of guid of the platform
                System.IO.File.WriteAllText($"{folderPath}/10-default.conf", rsyslog_config);
                }
            else 
            {
                // TODO
                // put windows stuff put here 
            }
        }

        // Allow user to download the script to run on their machine given the platformID
        public async Task Download(Guid platformId)
        {
            var platform = _context.Platforms.Where(x => x.Id == platformId)
                .Include(x => x.Settings)
                .Include(x => x.Siem).FirstOrDefault();
            
            if(platform == null)
                return;

            // create downloads folder if not yet existed
            if(!Directory.Exists("downloads"))
                Directory.CreateDirectory("downloads");

            // generate a new folder
            string folderPath = $"downloads/{platformId}";
            System.IO.Directory.CreateDirectory(folderPath);

            // Populate the template files
            SetupScriptFile(platform, folderPath);
            SetupLoggingConfigurationFile(platform, folderPath);

            // Zipping the config files
            Response.ContentType = "application/octet-stream";
            Response.Headers.Add("Content-Disposition", "attachment; filename=\"Install.zip\"");
            var zipFolderPath = Path.Combine(folderPath);
            var zipFilePaths = Directory.GetFiles(zipFolderPath);
            using (ZipArchive archive = new ZipArchive(Response.BodyWriter.AsStream(), ZipArchiveMode.Create))
            {
                // for all files in the folderPath we zip them
                foreach (var filePath in zipFilePaths)
                {
                    var filename = Path.GetFileName(filePath);
                    var entry = archive.CreateEntry(filename);
                    using (var entryStream = entry.Open())
                    using (var fileStream = System.IO.File.OpenRead(filePath))
                    {
                        // stream the zip to the client
                        await fileStream.CopyToAsync(entryStream);
                    }
                }
            }
        }

        // Allow user to upload their own template of a script to be used.
        public async Task<IActionResult> Upload(int os)
        {
            // decoding from base64 because script are stored as base64 due to security reasons
            // if file execution is achieved, attacker could not execute a base64 string even if the file is executable.

            string fileName = $"{((int)os).ToString()}.script";
            string b64Encoded = System.IO.File.ReadAllText(SCRIPT_DIRECTORY + fileName);
            string content =  Encoding.UTF8.GetString(Convert.FromBase64String(b64Encoded));

            ScriptUploadDTO scriptUpload = new ScriptUploadDTO{
                OperatingSystem = os,
                ScriptContent = content
            };

            // prepopulate the frontend with already existing script
            return View(scriptUpload);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(ScriptUploadDTO scriptUpload)
        {
            if(ModelState.IsValid)
            {
                // encode as base64 to limit attacker from populating this file with malicious code and executing it on the server.
                string fileName = $"{((int)scriptUpload.OperatingSystem).ToString()}.script";
                string b64Encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(scriptUpload.ScriptContent));
                System.IO.File.WriteAllText(SCRIPT_DIRECTORY + fileName, b64Encoded);
                return View(scriptUpload);
            }

            return View(scriptUpload);
        }
    }
}