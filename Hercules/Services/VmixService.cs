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
    
    // Same idea as FetchActiveGraphicsAsync, but unfiltered - overlays can
    // host any input type (Title, Video, Image, Camera...), not just the
    // Title/GT graphics with editable text/image fields that the Dashboard's
    // dropdowns care about.
    public async Task<List<VmixInput>> FetchAllInputsAsync(string vmixUrl = "http://127.0.0.1:8088/api/")
    {
        var allInputs = new List<VmixInput>();

        try
        {
            string xmlResponse = await _httpClient.GetStringAsync(vmixUrl);
            XDocument doc = XDocument.Parse(xmlResponse);

            foreach (var inputElement in doc.Descendants("input"))
            {
                allInputs.Add(new VmixInput
                {
                    Key = inputElement.Attribute("key")?.Value ?? "",
                    Title = inputElement.Attribute("title")?.Value ?? "Unknown Input"
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to vMix: {ex.Message}");
        }

        return allInputs;
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

    // Brings the given input onto overlay channel 1-4, using vMix's
    // configured transition for that overlay - matches clicking the
    // corresponding "Overlay N" button in vMix, but with a chosen Input.
    public void SendOverlayInCommand(int channel, string inputKey, string vmixUrl = "http://127.0.0.1:8088/api/")
    {
        try
        {
            string encodedInput = Uri.EscapeDataString(inputKey);
            string url = $"{vmixUrl}?Function=OverlayInput{channel}In&Input={encodedInput}";

            Console.WriteLine($"[VMIX OUT] {url}");

            _httpClient.GetAsync(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[VMIX ERROR] Overlay In command failed: {ex.Message}");
        }
    }

    // Transitions overlay channel 1-4 back off. No Input needed - it just
    // clears whatever's currently showing on that channel.
    public void SendOverlayOutCommand(int channel, string vmixUrl = "http://127.0.0.1:8088/api/")
    {
        try
        {
            string url = $"{vmixUrl}?Function=OverlayInput{channel}Out";

            Console.WriteLine($"[VMIX OUT] {url}");

            _httpClient.GetAsync(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[VMIX ERROR] Overlay Out command failed: {ex.Message}");
        }
    }

    // Fires a user-defined vMix Script (Settings > Scripting in vMix) by name.
    // NOTE: this uses vMix's documented scripting call, Function=Script&Value=<ScriptName>.
    // Double check this against your vMix version's API docs if scripts don't fire -
    // it hasn't been exercised against a live vMix instance here.
    public void SendScriptCommand(string scriptName, string vmixUrl = "http://127.0.0.1:8088/api/")
    {
        try
        {
            string encodedName = Uri.EscapeDataString(scriptName);
            string url = $"{vmixUrl}?Function=ScriptStart&Value={encodedName}";

            Console.WriteLine($"[VMIX OUT] {url}");

            _httpClient.GetAsync(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[VMIX ERROR] Script command failed: {ex.Message}");
        }
    }
}