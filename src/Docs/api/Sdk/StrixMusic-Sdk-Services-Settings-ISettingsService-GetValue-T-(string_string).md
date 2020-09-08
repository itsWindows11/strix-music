#### [StrixMusic.Sdk](./index.md 'index')
### [StrixMusic.Sdk.Services.Settings](./StrixMusic-Sdk-Services-Settings.md 'StrixMusic.Sdk.Services.Settings').[ISettingsService](./StrixMusic-Sdk-Services-Settings-ISettingsService.md 'StrixMusic.Sdk.Services.Settings.ISettingsService')
## ISettingsService.GetValue&lt;T&gt;(string, string) Method
Reads a value from the current [System.IServiceProvider](https://docs.microsoft.com/en-us/dotnet/api/System.IServiceProvider 'System.IServiceProvider') instance and returns its casting in the right type.  
```csharp
System.Threading.Tasks.Task<T> GetValue<T>(string key, string identifier);
```
#### Type parameters
<a name='StrixMusic-Sdk-Services-Settings-ISettingsService-GetValue-T-(string_string)-T'></a>
`T`  
The type of the object to retrieve.  
  
#### Parameters
<a name='StrixMusic-Sdk-Services-Settings-ISettingsService-GetValue-T-(string_string)-key'></a>
`key` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The key associated to the requested object.  
  
<a name='StrixMusic-Sdk-Services-Settings-ISettingsService-GetValue-T-(string_string)-identifier'></a>
`identifier` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
Identifies a unique version of this settings key.  
  
#### Returns
[System.Threading.Tasks.Task&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Threading.Tasks.Task-1 'System.Threading.Tasks.Task`1')[T](#StrixMusic-Sdk-Services-Settings-ISettingsService-GetValue-T-(string_string)-T 'StrixMusic.Sdk.Services.Settings.ISettingsService.GetValue&lt;T&gt;(string, string).T')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Threading.Tasks.Task-1 'System.Threading.Tasks.Task`1')  
A [System.Threading.Tasks.Task&lt;&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Threading.Tasks.Task-1 'System.Threading.Tasks.Task`1') that represents the value for the storage [key](#StrixMusic-Sdk-Services-Settings-ISettingsService-GetValue-T-(string_string)-key 'StrixMusic.Sdk.Services.Settings.ISettingsService.GetValue&lt;T&gt;(string, string).key') with the [identifier](#StrixMusic-Sdk-Services-Settings-ISettingsService-GetValue-T-(string_string)-identifier 'StrixMusic.Sdk.Services.Settings.ISettingsService.GetValue&lt;T&gt;(string, string).identifier').  