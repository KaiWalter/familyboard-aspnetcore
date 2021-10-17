﻿using FamilyBoard.Core.Cache;
using IntegratedCacheUtils.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace IntegratedCacheUtils.Stores
{
    public class FileSystemMsalAccountActivityStore : IMsalAccountActivityStore
    {
        private readonly string _activitiesPath;
        private readonly ILogger<FileSystemMsalAccountActivityStore> _logger;

        private List<MsalAccountActivity> _cache;

        public FileSystemMsalAccountActivityStore(ILogger<FileSystemMsalAccountActivityStore> logger,
        IOptions<DiskCacheOptions> options)
        {
            _activitiesPath = options.Value.ActivitiesPath;
            _logger = logger;
            this.ReadFromDisk();
        }

        private void ReadFromDisk()
        {
            if (File.Exists(_activitiesPath))
            {
                string json = File.ReadAllText(_activitiesPath);
                _cache = JsonSerializer.Deserialize<List<MsalAccountActivity>>(json);
                _logger.LogInformation($"MSAL activities loaded from cache {_activitiesPath}");
            }
            else
            {
                _cache = new List<MsalAccountActivity>();
                _logger.LogWarning($"no MSAL activities found in {_activitiesPath}");
                this.WriteToDisk();
            }
        }

        private void WriteToDisk()
        {
            var json = JsonSerializer.Serialize(_cache);
            File.WriteAllText(_activitiesPath, json);
            _logger.LogInformation($"MSAL activities written to cache {_activitiesPath}");
        }

        // Retrieve MsalAccountActivites that happened before a certain time ago
        public async Task<IEnumerable<MsalAccountActivity>> GetMsalAccountActivitesSince(DateTime lastActivityDate)
        {
            return _cache
                    .Where(x => x.FailedToAcquireToken == false
                        && x.LastActivity <= lastActivityDate);
        }

        // Retireve a specific user MsalAccountActivity
        public async Task<MsalAccountActivity> GetMsalAccountActivityForUser(string userPrincipalName)
        {
            return _cache
                    .Where(x => x.FailedToAcquireToken == false
                        && x.UserPrincipalName == userPrincipalName)
                    .FirstOrDefault();
        }

        public async Task<MsalAccountActivity> GetMsalAccountLastActivity()
        {
            return _cache
                    .Where(x => x.FailedToAcquireToken == false)
                    .OrderByDescending(x => x.LastActivity)
                    .FirstOrDefault();
        }



        // Setting the flag FailedToAcquireToken to true
        public async Task HandleIntegratedTokenAcquisitionFailure(MsalAccountActivity failedAccountActivity)
        {
            failedAccountActivity.FailedToAcquireToken = true;
            foreach (var accountActivity in _cache.Where(x => x.AccountCacheKey == failedAccountActivity.AccountCacheKey))
            {
                accountActivity.AccountIdentifier = failedAccountActivity.AccountIdentifier;
                accountActivity.AccountObjectId = failedAccountActivity.AccountObjectId;
                accountActivity.AccountTenantId = failedAccountActivity.AccountTenantId;
                accountActivity.UserPrincipalName = failedAccountActivity.UserPrincipalName;
                accountActivity.FailedToAcquireToken = failedAccountActivity.FailedToAcquireToken;
                accountActivity.LastActivity = failedAccountActivity.LastActivity;
            }
            this.WriteToDisk();
        }

        // Insert a new MsalAccountActivity case it doesnt exist, otherwise update the existing entry
        public async Task UpsertMsalAccountActivity(MsalAccountActivity msalAccountActivity)
        {
            if (_cache.Count(x => x.AccountCacheKey == msalAccountActivity.AccountCacheKey) != 0)
                foreach (var accountActivity in _cache.Where(x => x.AccountCacheKey == msalAccountActivity.AccountCacheKey))
                {
                    accountActivity.AccountIdentifier = msalAccountActivity.AccountIdentifier;
                    accountActivity.AccountObjectId = msalAccountActivity.AccountObjectId;
                    accountActivity.AccountTenantId = msalAccountActivity.AccountTenantId;
                    accountActivity.UserPrincipalName = msalAccountActivity.UserPrincipalName;
                    accountActivity.FailedToAcquireToken = msalAccountActivity.FailedToAcquireToken;
                    accountActivity.LastActivity = msalAccountActivity.LastActivity;
                }
            else
                _cache.Add(msalAccountActivity);

            this.WriteToDisk();
        }
    }
}