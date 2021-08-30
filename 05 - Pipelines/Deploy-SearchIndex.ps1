param(
    [Parameter(Mandatory)] $ResourcePrefix, 
    [Parameter(Mandatory)] $RootDirectory, 
    [Parameter(Mandatory)] $CognitiveServicesKey, 
    [Parameter(Mandatory)] $StorageConnectionString, 
    [Parameter(Mandatory)] $SearchServiceKey
)

$searchServiceName = $ResourcePrefix + "-search-service";
$storageContainerName = "documents";

$dataSourceName = $ResourcePrefix + "-datasource";
$skillsetName = $ResourcePrefix + "-skillset";
$indexName = $ResourcePrefix + "-index";
$indexerName = $ResourcePrefix + "-indexer";

function CreateSearchIndex
{
    Write-Host "Creating Search Index"; 

    function CallSearchAPI
    {
        param (
            [string]$url,
            [string]$body
        )

        $headers = @{
            'api-key' = $SearchServiceKey
            'Content-Type' = 'application/json' 
            'Accept' = 'application/json' 
        }
        $baseSearchUrl = "https://"+$searchServiceName+".search.windows.net"
        $fullUrl = $baseSearchUrl + $url

        Write-Host "Calling api: '"$fullUrl"'";
        Invoke-RestMethod -Uri $fullUrl -Headers $headers -Method Put -Body $body | ConvertTo-Json
    };

    # Create the datasource
    $dataSourceBody = Get-Content -Path "$RootDirectory\templates\base-datasource.json"
    $dataSourceBody = $dataSourceBody -replace "{{env_storage_connection_string}}", $StorageConnectionString      
    $dataSourceBody = $dataSourceBody -replace "{{env_storage_container}}", $storageContainerName        
    CallSearchAPI -url ("/datasources/"+$dataSourceName+"?api-version=2019-05-06") -body $dataSourceBody

    # Create the skillset
    $skillBody = Get-Content -Path "$RootDirectory\templates\base-skills.json"
    $skillBody = $skillBody -replace "{{cog_services_key}}", $CognitiveServicesKey  
    CallSearchAPI -url ("/skillsets/"+$skillsetName+"?api-version=2019-05-06") -body $skillBody

    # Create the index
    $indexBody = Get-Content -Path "$RootDirectory\templates\base-index.json"
    CallSearchAPI -url ("/indexes/"+$indexName+"?api-version=2019-05-06") -body $indexBody
    
    # Create the indexer
    $indexerBody = Get-Content -Path "$RootDirectory\templates\base-indexer.json"
    $indexerBody = $indexerBody -replace "{{datasource_name}}", $dataSourceName
    $indexerBody = $indexerBody -replace "{{skillset_name}}", $skillsetName   
    $indexerBody = $indexerBody -replace "{{index_name}}", $indexName   
    $indexerBody = $indexerBody -replace "{{execution_environment}}", 'Private'   
    CallSearchAPI -url ("/indexers/"+$indexerName+"?api-version=2019-05-06") -body $indexerBody
}

CreateSearchIndex;
