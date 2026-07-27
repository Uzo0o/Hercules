using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Hercules.Models;

namespace Hercules.Services;

public class VmixService
{
    private readonly HttpClient _httpClient = new HttpClient();

    public async Task<List<VmixInput>> FetchActiveGraphicsAsync(string vmixUrl = "http://127.0.0.1:8088/api/")
    {
        var activeGraphics = new List<VmixInput>();

        try
        {
            // Fetch the raw XML from vMix
            string xmlResponse = await _httpClient.GetStringAsync(vmixUrl);
            XDocument doc = XDocument.Parse(xmlResponse);

            // Look at every <input> node
            foreach (var inputElement in doc.Descendants("input"))
            {
                string type = inputElement.Attribute("type")?.Value ?? "";
                
                // We only care about graphics engines, ignore cameras/videos
                if (type == "GT" || type == "Title")
                {
                    var vmixInput = new VmixInput
                    {
                        Key = inputElement.Attribute("key")?.Value ?? "",
                        Title = inputElement.Attribute("title")?.Value ?? "Unknown Graphic"
                    };

                    // Extract all editable <text> nodes
                    foreach (var textNode in inputElement.Elements("text"))
                    {
                        vmixInput.Fields.Add(new VmixField { 
                            Name = textNode.Attribute("name")?.Value ?? "", 
                            Type = "Text" 
                        });
                    }

                    // Extract all editable <image> nodes
                    foreach (var imageNode in inputElement.Elements("image"))
                    {
                        vmixInput.Fields.Add(new VmixField { 
                            Name = imageNode.Attribute("name")?.Value ?? "", 
                            Type = "Image" 
                        });
                    }

                    // Only add it to our list if it actually has editable fields
                    if (vmixInput.Fields.Any())
                    {
                        activeGraphics.Add(vmixInput);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // In a real app, log this or send it to the UI (e.g., vMix is closed)
            Console.WriteLine($"Failed to connect to vMix: {ex.Message}");
        }

        return activeGraphics;
    }
    
    public void SendSetTextCommand(string inputKey, string fieldName, string value, string vmixUrl = "http://127.0.0.1:8088/api/")
    {
        try
        {
            string encodedValue = Uri.EscapeDataString(value);
            string encodedName = Uri.EscapeDataString(fieldName);
        
            string url = $"{vmixUrl}?Function=SetText&Input={inputKey}&SelectedName={encodedName}&Value={encodedValue}";

            // --- NEW DEBUG LINE ---
            Console.WriteLine($"[VMIX OUT] {url}");

            _httpClient.GetAsync(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[VMIX ERROR] Command failed: {ex.Message}");
        }
    }
}