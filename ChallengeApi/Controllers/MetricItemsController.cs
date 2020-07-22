using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChallengeApi.Models;
using Hazelcast.Client;
using Hazelcast.Core;
using Hazelcast.Config;


namespace ChallengeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetricItemsController : ControllerBase

    {
        private readonly MetricContext _context;

        public MetricItemsController(MetricContext context)
        {
            _context = context;
        }

        // GET: api/MetricItems/apiuser@example.com
        [HttpGet("{userName}")]
        public async Task<ActionResult<IEnumerable<MetricItem>>> GetMetricItems(string userName)
        {   
            // We should clear context each time         
            ClearContext();
    
            string hazelcastIP = Environment.GetEnvironmentVariable("ENV_HIP");
            if(string.IsNullOrEmpty(hazelcastIP)) {
                hazelcastIP = "127.0.0.1";
            }
            var cfg = new ClientConfig();
            cfg.GetNetworkConfig().AddAddress(hazelcastIP);
            var client = HazelcastClient.NewHazelcastClient(cfg);
           
            IMap<string, double> mapMetricsAggregatedData = client.GetMap<string,double>("metrics-aggregated-data");
            ICollection<string> keys = mapMetricsAggregatedData.KeySet();
            MetricItem metricItem;
            foreach (var key in keys) {
                string[] keyParts = key.Split('*');
                if(String.Equals(userName, keyParts[0]))
                {
                    metricItem = new MetricItem();
                    metricItem.Id = key;
                    metricItem.Value = mapMetricsAggregatedData.Get(key);;

                    _context.MetricItems.Add(metricItem);
                    await _context.SaveChangesAsync();
                }
            }

            return await _context.MetricItems.ToListAsync();
        }

        // GET: api/MetricItems/apiuser@example.com/googlefit
        [HttpGet("{userName}/{shimKey}")]
        public async Task<ActionResult<IEnumerable<MetricItem>>> GetMetricItems(string userName, string shimKey)
        {            
            ClearContext();
    
            string hazelcastIP = Environment.GetEnvironmentVariable("ENV_HIP");
            if(string.IsNullOrEmpty(hazelcastIP)) {
                hazelcastIP = "127.0.0.1";
            }
            var cfg = new ClientConfig();
            cfg.GetNetworkConfig().AddAddress(hazelcastIP);
            var client = HazelcastClient.NewHazelcastClient(cfg);

            IMap<string, double> mapMetricsAggregatedData = client.GetMap<string,double>("metrics-aggregated-data");
            ICollection<string> keys = mapMetricsAggregatedData.KeySet();
            MetricItem metricItem;
            foreach (var key in keys) {
                string[] keyParts = key.Split('*');
                if(String.Equals(userName, keyParts[0]) && String.Equals(shimKey, keyParts[1]))
                {
                    metricItem = new MetricItem();
                    metricItem.Id = key;
                    metricItem.Value = mapMetricsAggregatedData.Get(key);;

                    _context.MetricItems.Add(metricItem);
                    await _context.SaveChangesAsync();
                }
            }

            return await _context.MetricItems.ToListAsync();
        }

        // GET: api/MetricItems/apiuser@example.com/googlefit/body_weight
        [HttpGet("{userName}/{shimKey}/{endpoint}")]
        public async Task<ActionResult<MetricItem>> GetMetricItem(string userName, string shimKey, string endpoint)
        {
            var metricItem = new MetricItem();
            await Task.Run(() => 
            {
                            
            string hazelcastIP = Environment.GetEnvironmentVariable("ENV_HIP");
            if(string.IsNullOrEmpty(hazelcastIP)) {
                hazelcastIP = "127.0.0.1";
            }
            var cfg = new ClientConfig();
            cfg.GetNetworkConfig().AddAddress(hazelcastIP);
            var client = HazelcastClient.NewHazelcastClient(cfg);

            IMap<string, double> mapMetricsAggregatedData = client.GetMap<string,double>("metrics-aggregated-data");
            ICollection<string> keys = mapMetricsAggregatedData.KeySet();
            foreach (var key in keys) {
                string[] keyParts = key.Split('*');
                if(String.Equals(userName, keyParts[0]) && String.Equals(shimKey, keyParts[1]) && String.Equals(endpoint, keyParts[2]))
                {
                    metricItem = new MetricItem();
                    metricItem.Id = key;
                    metricItem.Value = mapMetricsAggregatedData.Get(key);
                }
            }
            });

            return metricItem; 
        }

        // GET: api/MetricItems/GetByUserNameAndEndpoint/apiuser@example.com/googlefit/body_weight
        [Route("[action]/{userName}/{endpoint}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MetricItem>>> GetByUserNameAndEndpoint(string userName, string endpoint)
        {            
            ClearContext();
    
            string hazelcastIP = Environment.GetEnvironmentVariable("ENV_HIP");
            if(string.IsNullOrEmpty(hazelcastIP)) {
                hazelcastIP = "127.0.0.1";
            }
            var cfg = new ClientConfig();
            cfg.GetNetworkConfig().AddAddress(hazelcastIP);
            var client = HazelcastClient.NewHazelcastClient(cfg);

            IMap<string, double> mapMetricsAggregatedData = client.GetMap<string,double>("metrics-aggregated-data");
            ICollection<string> keys = mapMetricsAggregatedData.KeySet();
            MetricItem metricItem;
            foreach (var key in keys) {
                string[] keyParts = key.Split('*');
                if(String.Equals(userName, keyParts[0]) && String.Equals(endpoint, keyParts[2]))
                {
                    metricItem = new MetricItem();
                    metricItem.Id = key;
                    metricItem.Value = mapMetricsAggregatedData.Get(key);;

                    _context.MetricItems.Add(metricItem);
                    await _context.SaveChangesAsync();
                }
            }

            return await _context.MetricItems.ToListAsync();
        }

/*         // GET: api/MetricItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MetricItem>> GetMetricItem(long id)
        {
            var metricItem = await _context.MetricItems.FindAsync(id);

            if (metricItem == null)
            {
                return NotFound();
            }

            return metricItem;
        } */

        // PUT: api/MetricItems/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMetricItem(string id, MetricItem metricItem)
        {
            if (id != metricItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(metricItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MetricItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/MetricItems
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<MetricItem>> PostMetricItem(MetricItem metricItem)
        {
            _context.MetricItems.Add(metricItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMetricItem", new { Id = metricItem.Id }, metricItem);
        }

        // DELETE: api/MetricItems/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<MetricItem>> DeleteMetricItem(long id)
        {
            var metricItem = await _context.MetricItems.FindAsync(id);
            if (metricItem == null)
            {
                return NotFound();
            }

            _context.MetricItems.Remove(metricItem);
            await _context.SaveChangesAsync();

            return metricItem;
        }

        private bool MetricItemExists(string id)
        {
            return _context.MetricItems.Any(e => e.Id == id);
        }
        
        private void ClearContext() {
            
            foreach(var metricItem in _context.MetricItems)
            {
                _context.MetricItems.Remove(metricItem);
            }
            _context.SaveChanges();
        }
    }
}
