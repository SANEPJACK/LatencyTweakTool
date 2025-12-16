using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace LatencyTweakTool
{
    internal static class AuthService
    {
        private const string FirebaseDbUrl = "https://redskull-8888-default-rtdb.asia-southeast1.firebasedatabase.app";
        private const string FirebaseAuthToken = ""; // rules เปิดสาธารณะ ไม่ต้องใช้ token
        private static readonly HttpClient Http = new HttpClient();

        public static async Task<AuthResult> VerifyOrCreateAsync()
        {
            string uuid = MachineId.GetMachineUuid();
            var record = await FetchOrCreateCustomerAsync(uuid).ConfigureAwait(false);

            if (record == null)
            {
                throw new InvalidOperationException("ไม่สามารถดึงข้อมูลสิทธิ์จาก Firebase");
            }

            if (!record.status)
            {
                return new AuthResult(uuid, false, record.name, record.plan);
            }

            record.program ??= "latency";
            record.lastSeen = DateTime.UtcNow.ToString("o");
            await SaveCustomerByUuidAsync(uuid, record).ConfigureAwait(false);

            return new AuthResult(uuid, true, record.name, record.plan);
        }

        private static async Task<CustomerRecord?> FetchOrCreateCustomerAsync(string uuid)
        {
            string url = BuildFirebaseCustomerUrl(uuid);

            CustomerRecord? record = await TryGetCustomerAsync(url).ConfigureAwait(false);
            if (record != null)
            {
                return record;
            }

            record = new CustomerRecord
            {
                name = "ผู้ใช้ใหม่",
                plan = "member",
                status = false,
                program = "latency",
                createdAt = DateTime.UtcNow.ToString("o"),
                lastSeen = DateTime.UtcNow.ToString("o")
            };

            await SaveCustomerAsync(url, record).ConfigureAwait(false);
            return record;
        }

        private static async Task<CustomerRecord?> TryGetCustomerAsync(string url)
        {
            try
            {
                var response = await Http.GetAsync(url).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(body) || body.Trim() == "null")
                {
                    return null;
                }

                return JsonSerializer.Deserialize<CustomerRecord>(body);
            }
            catch
            {
                return null;
            }
        }

        private static async Task SaveCustomerByUuidAsync(string uuid, CustomerRecord record)
        {
            string url = BuildFirebaseCustomerUrl(uuid);
            await SaveCustomerAsync(url, record).ConfigureAwait(false);
        }

        private static async Task SaveCustomerAsync(string url, CustomerRecord record)
        {
            string payload = JsonSerializer.Serialize(record);
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await Http.PutAsync(url, content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }

        private static string BuildFirebaseCustomerUrl(string uuid)
        {
            string baseUrl = FirebaseDbUrl.TrimEnd('/');
            string path = "/customers/" + Uri.EscapeDataString(uuid) + ".json";

            if (!string.IsNullOrWhiteSpace(FirebaseAuthToken))
            {
                return baseUrl + path + "?auth=" + FirebaseAuthToken;
            }

            return baseUrl + path;
        }

        private static class MachineId
        {
            public static string GetMachineUuid()
            {
                try
                {
                    using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
                    string? machineGuid = key?.GetValue("MachineGuid")?.ToString();
                    if (!string.IsNullOrWhiteSpace(machineGuid))
                    {
                        return machineGuid;
                    }
                }
                catch
                {
                    // fallback below
                }

                return Environment.MachineName;
            }
        }

        internal class CustomerRecord
        {
            public string? name { get; set; }
            public string? plan { get; set; }
            public bool status { get; set; }
            public string? program { get; set; }
            public string? expiry { get; set; }
            public string? createdAt { get; set; }
            public string? lastSeen { get; set; }
        }
    }

    public record AuthResult(string Uuid, bool IsAuthorized, string? Name, string? Plan);
}
