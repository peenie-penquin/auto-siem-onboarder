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
using System.Security.Cryptography;

namespace test_auth.Controllers
{
    public class ApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [NonAction]
        // Helper function to compute the has of the node object
        static string SHA256_Node(Node node)
        {
            string inputString = node.Hostname + node.IpAddress + node.OS;

            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(inputString));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        [HttpPost]
        // Script is going to call this route to update the node details back to the server
        public string AddNodes([FromBody] NodeDTO nodeDetails) // Get input from JSON
        {
            // validate the input  
            // further validation rules please see NodeDTO defintion
            if(!ModelState.IsValid)
                return "Bad Request";


            // lookup the corresponding platform by using the id field; which contains the platform id
            var platform = _context.Platforms
                .Where(x => x.Id == nodeDetails.Id)
                .Include(x => x.Nodes).FirstOrDefault();

            if (platform == null)
                return "NOTFOUND";

            // create a an empty list if isn't already
            if (platform.Nodes == null)
                platform.Nodes = new List<Node>();

            // create a new node object
            var newNode = new Node
            {
                Hostname = nodeDetails.Hostname.Trim(),
                IpAddress = nodeDetails.IpAddress.Trim(),
                OS = nodeDetails.OS.Trim(),
                isSiemReachable = true // todo implement this in script; 
                //future works could ping the siem from the machine to let us know if they could contact the siem.
            };

            // Prevent duplicates from being added
            // We compute the hash of each node and compare against the new node
            // If same, it is a duplicate
            string newNodeHash = SHA256_Node(newNode);
            bool isDuplicate = false;
            foreach (Node item in platform.Nodes)
            {
                if (newNodeHash == SHA256_Node(item))
                {
                    isDuplicate = true;
                    break;
                }
            }

            if (isDuplicate)
                return "Already Added";

            platform.Nodes.Add(newNode); // add to database
            _context.SaveChanges(); // save database

            return $"{nodeDetails.Hostname}, {nodeDetails.Id}, {nodeDetails.IpAddress}, {nodeDetails.OS}";
        }
    }
}