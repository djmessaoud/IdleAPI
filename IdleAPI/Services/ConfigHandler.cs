using IdleAPI.Data;
using IdleAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;

namespace IdleAPI.Services
{
    public class ConfigHandler
    {
        private readonly IdleContext _context;
        //encapsulating data to prevent modification maybe using cheatengines
        public decimal reward_value { get; private set; }
        public double session_time { get; private set; }

        // We can use Dictionary for more simplicity and flexibility
        // 
        // private readonly Dictionary<string, string> _config = new Dictionary<string, string>(); //
        //
        public ConfigHandler(IdleContext context)
        {
            _context = context;
            LoadConfig(); //load config on startup
        }
        public async Task LoadConfig()
        {
            reward_value = decimal.Parse(await GetConfig("reward_value"));
            session_time = double.Parse(await GetConfig("session_time"));
        }
        //get config value from database and if not found set default values
        public async Task<string> GetConfig(string key)
        {
            var config = await _context.IdleConfig.FirstOrDefaultAsync(c => c.Key == key);
            if (config == null)
            {
                string value = "";
                if (key == "reward_value")
                    value = "300";
                else if (key == "session_time")
                    value = "60";
                await SetConfig(key, value);
                return value;
            }
            return config.Value;
        }
        public async Task<string> SetConfig(string key, string value)
        {
            try
            {
                var config = await _context.IdleConfig.FirstOrDefaultAsync(c => c.Key == key);
                if (config == null)
                {
                   await _context.IdleConfig.AddAsync(new Config { Key = key, Value = value });
                }
                else
                {
                    config.Value = value;
                }
                await _context.SaveChangesAsync();
                //update local values 
                if (key == "reward_value")
                    reward_value = decimal.Parse(value);
                else if (key == "session_time")
                    session_time = double.Parse(value);
                return "1";
            }
            catch (Exception e)
            {
               
                return e.Message;
            }
        }
       
    }
}
