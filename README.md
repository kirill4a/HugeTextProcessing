# Usage

`dotnet HugeTextProcessing.Console.dll`

output:
```Description:
  Huge text generator CLI

Usage:
  HugeTextProcessing.Console [command] [options]

Options:
  -?, -h, --help  Show help and usage information
  --version       Show version information

Commands:
  generate  Generates a multiline text file
  sort      Sorts a multiline text file
```  
  
# To generate file  

### display help usage
`dotnet HugeTextProcessing.Console.dll generate -h`

output:
```Description:
  Generates a multiline text file

Usage:
  HugeTextProcessing.Console generate [options]

Options:
  -s, --size <size>  File size (e.g. 1GB, 500MB, 10KB); default 1MB in case of not specified. [default: FileSize { Bytes = 1048576 }]
  -?, -h, --help     Show help and usage information
```

### generates 1 GB file in temp directory
`dotnet HugeTextProcessing.Console.dll generate --size 1GB`


# To sort file  

### display help usage
`dotnet HugeTextProcessing.Console.dll sort -h`

output:
```Description:
  Sorts a multiline text file

Usage:
  HugeTextProcessing.Console sort [options]

Options:
  -s, --source <source> (REQUIRED)            The full path to file being sorted.
  -d, --destination <destination> (REQUIRED)  The full path to result sorted file.
  -?, -h, --help                              Show help and usage information
```
  
### sorts specified file and save results in another one  
`dotnet HugeTextProcessing.Console.dll sort --source QWERTY.txt -d ASDFGH.txt`
