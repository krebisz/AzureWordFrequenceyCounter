AzureFunctionsApp

1. Total Time Taken: (13-14 hours)
*Started by developing a local app to do the processing
*Created the required resources and accounts within Azure
*Set up a flow to deploy test and further develop code with Azure, PostMan & Git 
*Built out the Azure functions with the above code to perform the required operations
*Did some testing, reworking, refactoring and design decisions based on current constraints (see below)



2. Working implementation: (https://azurefunctionsfrequencycounter.azurewebsites.net/api/FrequencyFunction)
*Developed and ran in conjunction with Visual Studio 2022
*Used the PostMan application to test both locally and the deployed app in azure
*The abpve was performed by POSTING a file as form-data in the body. A plain text file is uploaded from a local drive
*If to be deployed, it requires the usual Azure stuff to be set up (The configuration stuff is obviously unique to the instances created)




3. Constraints: (On-time vs. Functionality vs. Quality)
*Attempted to provide all functionality, even in some limited sense or requiring more robustness
*Performance Analytics: Microsoft.ApplicationInsights (Telemetry: TelemetryClient) was to be used, but time has made it more practical to do a simple timer to complete ops. Insights seem simple enough to add though from within Azure on the dashboard
*Output: Basic output provided with a string that is built in different parts of the code ->DEFINATELY not the way to go about it (should be a single point that takes in parameterized values)
*Tiered architecture: Individual projects to handle data; models and logic requires more time. Also  to do things conforming to proper SOLID principles
*Regex cleanup: Some words not correctl parsed. Need more symbols and robust processing and would require more debugging to get completely right
*Performance: Wanted to take the ordered Word List and provide an O(n) type of performance by counting the number of occurences in it, other possibilities like using hash tables, sub-lists (like a merge sort) and parrelel processing also exist
*Testing: Much is required, no time for unit tests or the like
*Resources: Read and got ideas from much more than those listed in the code, but the primary ones where I actually copied snippets are referenced
*Configuration: Should be injected from a config file (or stored in Azure) and protected from clear-text


