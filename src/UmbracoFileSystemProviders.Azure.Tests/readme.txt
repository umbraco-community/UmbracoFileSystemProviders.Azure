For testing SAS token credentials I have introduce two compile settings:
SAS Container Level

SAS Account Level

You see them in "Standard" toolbar or via "Configuration Manager" under "Build" menu.
Within file AzureBlobFileSystemBase.cs of the Tests project in method AzureBlobFileSystemTestsBase.CreateAzureBlobFileSystem you find these compiler switches.
You have to replace the [accountName], [sasAccountConnectionString] and [sasQueryStringWithoutLeadingQuestionMark] by previously configured values.
You can easily use "Microsoft Azure Storage Explorer" (download: https://azure.microsoft.com/en-us/features/storage-explorer/) for this. 
Currently I recommend the standalone application, but a preview is also in the Azure portal available.