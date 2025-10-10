using Amazon;
using Amazon.Extensions.Configuration.SystemsManager;
using Amazon.Extensions.NETCore.Setup;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BrightNess.Library.Configuration
{
    public static class AwsSecretsManagerConfigurationExtensions
    {
        private const string SecretsEnvVariable = "AWS_SECRETS_MANAGER_SECRETS";
        private const string AlternateSecretsEnvVariable = "AWS__SecretsManager__Secrets";
        private const string DefaultSecretsPathPrefix = "/aws/reference/secretsmanager/";

        public static IConfigurationManager AddSecretsManager(this IConfigurationManager configurationManager, Action<AwsSecretsManagerOptions>? configure = null)
        {
            if (configurationManager is null) throw new ArgumentNullException(nameof(configurationManager));

            _ = AddSecretsManager((IConfigurationBuilder)configurationManager, configure);
            return configurationManager;
        }

        public static IConfigurationBuilder AddSecretsManager(this IConfigurationBuilder builder, Action<AwsSecretsManagerOptions>? configure = null)
        {
            if (builder is null) throw new ArgumentNullException(nameof(builder));

            var snapshot = BuildSnapshot(builder);

            var options = new AwsSecretsManagerOptions();
            snapshot.GetSection(AwsSecretsManagerOptions.ConfigurationSectionName).Bind(options);
            configure?.Invoke(options);

            CollectSecretsFromEnvironment(options);

            options.Normalize();

            if (options.Secrets.Count == 0)
            {
                return builder;
            }

            foreach (var secret in options.Secrets)
            {
                if (!TryCreateSourceOptions(options, secret, snapshot, out var sourceOptions))
                {
                    continue;
                }

                builder.AddSystemsManager(source =>
                {
                    source.Path = sourceOptions.Path;
                    source.Optional = sourceOptions.Optional;
                    if (sourceOptions.ReloadAfter.HasValue)
                    {
                        source.ReloadAfter = sourceOptions.ReloadAfter;
                    }

                    if (sourceOptions.AwsOptions is not null)
                    {
                        source.AwsOptions = sourceOptions.AwsOptions;
                    }

                    source.ParameterProcessor ??= new JsonOrStringParameterProcessor();
                });
            }

            return builder;
        }

        private static IConfiguration BuildSnapshot(IConfigurationBuilder builder)
        {
            var snapshotBuilder = new ConfigurationBuilder();
            foreach (var source in builder.Sources)
            {
                snapshotBuilder.Add(source);
            }

            return snapshotBuilder.Build();
        }

        private static void CollectSecretsFromEnvironment(AwsSecretsManagerOptions options)
        {
            options.Region ??= GetNonEmptyEnvironmentVariable("AWS__SecretsManager__Region")
                               ?? GetNonEmptyEnvironmentVariable("AWS_SECRETS_MANAGER_REGION");
            options.PathPrefix ??= GetNonEmptyEnvironmentVariable("AWS__SecretsManager__PathPrefix")
                                   ?? GetNonEmptyEnvironmentVariable("AWS_SECRETS_MANAGER_PATH_PREFIX");

            var optionalEnv = GetNonEmptyEnvironmentVariable("AWS__SecretsManager__Optional")
                              ?? GetNonEmptyEnvironmentVariable("AWS_SECRETS_MANAGER_OPTIONAL");
            if (!string.IsNullOrWhiteSpace(optionalEnv) && bool.TryParse(optionalEnv, out var optionalOverride))
            {
                options.Optional = optionalOverride;
            }

            var reloadAfterEnv = GetNonEmptyEnvironmentVariable("AWS__SecretsManager__ReloadAfter")
                                 ?? GetNonEmptyEnvironmentVariable("AWS_SECRETS_MANAGER_RELOAD_AFTER");
            if (!string.IsNullOrWhiteSpace(reloadAfterEnv) && TimeSpan.TryParse(reloadAfterEnv, out var reloadAfter))
            {
                options.ReloadAfter = reloadAfter;
            }

            var envValue = Environment.GetEnvironmentVariable(SecretsEnvVariable);
            if (string.IsNullOrWhiteSpace(envValue))
            {
                envValue = Environment.GetEnvironmentVariable(AlternateSecretsEnvVariable);
            }

            if (string.IsNullOrWhiteSpace(envValue))
            {
                return;
            }

            foreach (var token in envValue.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = token.Trim();
                if (trimmed.Length == 0)
                {
                    continue;
                }

                options.Secrets.Add(new AwsSecretDescriptor { SecretId = trimmed });
            }

            var structuredDescriptors = ReadStructuredSecretDescriptorsFromEnvironment();
            foreach (var descriptor in structuredDescriptors)
            {
                options.Secrets.Add(descriptor);
            }
        }

        private static IEnumerable<AwsSecretDescriptor> ReadStructuredSecretDescriptorsFromEnvironment()
        {
            var envVars = Environment.GetEnvironmentVariables();
            var prefix = "AWS__SecretsManager__Secrets__";
            var fallbackPrefix = "AWS_SECRETS_MANAGER_SECRETS_";

            var dictionary = new Dictionary<string, AwsSecretDescriptor>(StringComparer.OrdinalIgnoreCase);

            foreach (DictionaryEntry entry in envVars)
            {
                if (entry.Key is not string key || entry.Value is not string value)
                {
                    continue;
                }

                if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    ProcessStructuredKey(key.Substring(prefix.Length), value, dictionary);
                }
                else if (key.StartsWith(fallbackPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    ProcessStructuredKey(key.Substring(fallbackPrefix.Length), value, dictionary);
                }
            }

            return dictionary
                .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
                .Select(pair => pair.Value);
        }

        private static void ProcessStructuredKey(string keyRemainder, string value, IDictionary<string, AwsSecretDescriptor> descriptors)
        {
            if (string.IsNullOrEmpty(keyRemainder))
            {
                return;
            }

            var parts = keyRemainder.Split(new[] { "__" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                return;
            }

            var index = parts[0];
            var property = string.Join("__", parts.Skip(1));

            if (!descriptors.TryGetValue(index, out var descriptor))
            {
                descriptor = new AwsSecretDescriptor();
                descriptors[index] = descriptor;
            }

            switch (property.ToLowerInvariant())
            {
                case "secretid":
                    descriptor.SecretId = value;
                    break;
                case "parameterpath":
                    descriptor.ParameterPath = value;
                    break;
                case "pathprefix":
                    descriptor.PathPrefix = value;
                    break;
                case "region":
                    descriptor.Region = value;
                    break;
                case "optional":
                    if (bool.TryParse(value, out var optional))
                    {
                        descriptor.Optional = optional;
                    }
                    break;
                case "reloadafter":
                    if (TimeSpan.TryParse(value, out var reloadAfter))
                    {
                        descriptor.ReloadAfter = reloadAfter;
                    }
                    break;
            }
        }

        private static string? GetNonEmptyEnvironmentVariable(string variableName)
        {
            var value = Environment.GetEnvironmentVariable(variableName);
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private static bool TryCreateSourceOptions(
            AwsSecretsManagerOptions root,
            AwsSecretDescriptor secret,
            IConfiguration snapshot,
            out SystemsManagerSourceOptions? options)
        {
            options = null;

            var path = ResolvePath(root, secret);
            if (path is null)
            {
                return false;
            }

            var reloadAfter = secret.ReloadAfter ?? root.ReloadAfter;
            var optional = secret.Optional ?? root.Optional;
            var region = secret.Region ?? root.Region ?? snapshot["AWS:Region"] ?? Environment.GetEnvironmentVariable("AWS_REGION") ?? Environment.GetEnvironmentVariable("AWS_DEFAULT_REGION");

            AWSOptions? awsOptions = null;
            if (!string.IsNullOrWhiteSpace(region))
            {
                awsOptions = new AWSOptions { Region = RegionEndpoint.GetBySystemName(region) };
            }

            options = new SystemsManagerSourceOptions(path, optional, reloadAfter, awsOptions);
            return true;
        }

        private static string? ResolvePath(AwsSecretsManagerOptions root, AwsSecretDescriptor secret)
        {
            if (!string.IsNullOrWhiteSpace(secret.ParameterPath))
            {
                return secret.ParameterPath;
            }

            if (string.IsNullOrWhiteSpace(secret.SecretId))
            {
                return null;
            }

            var prefix = secret.PathPrefix ?? root.PathPrefix ?? DefaultSecretsPathPrefix;

            if (!prefix.StartsWith("/aws/reference/secretsmanager/", StringComparison.OrdinalIgnoreCase))
            {
                prefix = $"{DefaultSecretsPathPrefix}{prefix.Trim('/')}";
            }

            if (!prefix.EndsWith("/", StringComparison.Ordinal))
            {
                prefix += "/";
            }

            return $"{prefix}{secret.SecretId}";
        }

        private sealed record SystemsManagerSourceOptions(string Path, bool Optional, TimeSpan? ReloadAfter, AWSOptions? AwsOptions);
    }

    public sealed class AwsSecretsManagerOptions
    {
        internal const string ConfigurationSectionName = "AWS:SecretsManager";

        public string? Region { get; set; }
        public string? PathPrefix { get; set; }
        public bool Optional { get; set; } = true;
        public TimeSpan? ReloadAfter { get; set; }
        public IList<AwsSecretDescriptor> Secrets { get; } = new List<AwsSecretDescriptor>();

        internal void Normalize()
        {
            for (var i = Secrets.Count - 1; i >= 0; i--)
            {
                var descriptor = Secrets[i];
                if (descriptor is null)
                {
                    Secrets.RemoveAt(i);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(descriptor.SecretId) && string.IsNullOrWhiteSpace(descriptor.ParameterPath))
                {
                    Secrets.RemoveAt(i);
                    continue;
                }
            }

            var distinct = Secrets
                .GroupBy(secret => secret.ParameterPath ?? secret.SecretId, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();

            if (distinct.Count != Secrets.Count)
            {
                Secrets.Clear();
                foreach (var secret in distinct)
                {
                    Secrets.Add(secret);
                }
            }
        }
    }

    public sealed class AwsSecretDescriptor
    {
        public string? SecretId { get; set; }
        public string? ParameterPath { get; set; }
        public string? PathPrefix { get; set; }
        public bool? Optional { get; set; }
        public TimeSpan? ReloadAfter { get; set; }
        public string? Region { get; set; }
    }
}
