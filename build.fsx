// --------------------------------------------------------------------------------------
// FAKE build script 
// --------------------------------------------------------------------------------------

#r @"packages/FAKE/tools/FakeLib.dll"
open Fake 
open Fake.Git
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open System
open System.IO
open System.Text.RegularExpressions

// Information about the project are used
//  - for version and project name in generated AssemblyInfo file
//  - by the generated NuGet package 
//  - to run tests and to publish documentation on GitHub gh-pages
//  - for documentation, you also need to edit info in "docs/tools/generate.fsx"

// The name of the project 
// (used by attributes in AssemblyInfo, name of a NuGet package and directory in 'src')
let project = "Paket.VisualStudio"

// Short summary of the project
// (used as description in AssemblyInfo and as a short summary for NuGet package)
let summary = "Manage your Paket dependencies from Visual Studio!"

// Longer description of the project
// (used as a description for NuGet package; line breaks are automatically cleaned up)
let description = "Manage your Paket dependencies from Visual Studio!"
// List of author names (for NuGet package)
let authors = [ "Igal Tabachnik"; "Steffen Forkmann" ]
// Tags for your project (for NuGet package)
let tags = "package management paket nuget"

// File system information 
// (<solutionFile>.sln is built during the building process)
let solutionFile  = "Paket.VisualStudio"
// Pattern specifying assemblies to be tested using NUnit
let testAssemblies = "tests/**/bin/Release/*Tests*.dll"

// Git configuration (used for publishing documentation in gh-pages branch)
// The profile where the project is posted 
let gitOwner = "fsprojects"
let gitHome = "https://github.com/" + gitOwner

// The name of the project on GitHub
let gitName = "Paket.VisualStudio"
let cloneUrl = "https://github.com/fsprojects/Paket.VisualStudio.git"

// Read additional information from the release notes document
Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
let release = parseReleaseNotes (IO.File.ReadAllLines "RELEASE_NOTES.md")

let isAppVeyorBuild = environVar "APPVEYOR" <> null
let buildVersion = sprintf "%s-a%s" release.NugetVersion (DateTime.UtcNow.ToString "yyMMddHHmm")

let buildDir = "bin"
let vsixDir = "bin/vsix"
let tempDir = "temp"
let buildMergedDir = buildDir @@ "merged"

Target "BuildVersion" (fun _ ->
    Shell.Exec("appveyor", sprintf "UpdateBuild -Version \"%s\"" buildVersion) |> ignore
)

// Generate assembly info files with the right version & up-to-date information
Target "AssemblyInfo" (fun _ ->
  let shared =
      [ Attribute.Product project
        Attribute.Description summary
        Attribute.Version release.AssemblyVersion
        Attribute.FileVersion release.AssemblyVersion ] 

  CreateCSharpAssemblyInfo "src/Paket.VisualStudio/Properties/AssemblyInfo.cs"
      (Attribute.InternalsVisibleTo "Paket.VisualStudio.Tests" :: Attribute.Title "Paket.VisualStudio" :: shared)

  let manifest = "src/Paket.VisualStudio/source.extension.vsixmanifest"
  File.WriteAllLines(
      manifest,
      File.ReadAllLines manifest
      |> Array.map (fun l -> if l.Contains("<Version>") then sprintf "    <Version>%s</Version>" release.NugetVersion else l))
      
)

// --------------------------------------------------------------------------------------
// Clean build results

Target "Clean" (fun _ ->
    CleanDirs [buildDir; vsixDir; tempDir; "nuget"]
)

Target "CleanDocs" (fun _ ->
    CleanDirs ["docs/output"]
)

// --------------------------------------------------------------------------------------
// Build library & test project

Target "Build" (fun _ ->
    // We would like to build only one solution
    !! (solutionFile + ".sln")
    |> MSBuildRelease "" "Rebuild"
    |> ignore
)

Target "CleanVSIX" (fun _ ->
    ZipHelper.Unzip vsixDir "bin/Paket.VisualStudio.vsix"
    let regex = Regex("bin")
    let filesToKeep =
      Directory.GetFiles("bin", "*.dll")
      |> Seq.map (fun fileName -> regex.Replace(fileName, vsixDir, 1))
    let filesToDelete = 
      Seq.fold (--) (!! "bin/vsix/*.dll") filesToKeep
        ++ "bin/vsix/Microsoft.VisualStudio*"
        ++ "bin/vsix/Microsoft.Build*"
    DeleteFiles filesToDelete

    CreateDir buildMergedDir

    let filesToPack =
        ["Paket.VisualStudio.dll"; "Paket.Core.dll"; "FSharp.Core.dll"; "Newtonsoft.Json.dll"; 
         "ReactiveUI.dll"; "ReactiveUI.Events.dll"; "Splat.dll"; "System.Reactive.Core.dll"; "System.Reactive.Interfaces.dll"; "System.Reactive.Linq.dll"; "System.Reactive.PlatformServices.dll"; "System.Reactive.Windows.Threading.dll"]
        |> List.map (fun l -> vsixDir @@ l)

    let toPack = filesToPack |> separated " "

    let result =
        ExecProcess (fun info ->
            info.FileName <- currentDirectory @@ "packages" @@ "ILRepack" @@ "tools" @@ "ILRepack.exe"
            info.Arguments <- sprintf "/verbose /lib:%s /ver:%s /out:%s %s" vsixDir release.AssemblyVersion (buildMergedDir @@ "Paket.VisualStudio.dll") toPack
            ) (TimeSpan.FromMinutes 5.)

    if result <> 0 then failwithf "Error during ILRepack execution."

    DeleteFiles filesToPack
    CopyFile vsixDir (buildMergedDir @@ "Paket.VisualStudio.dll")

    ZipHelper.Zip vsixDir "bin/Paket.VisualStudio.vsix" (!! "bin/vsix/**")
)

// --------------------------------------------------------------------------------------
// Generate the documentation

Target "GenerateDocs" (fun _ ->
    executeFSIWithArgs "docs/tools" "generate.fsx" ["--define:RELEASE"] [] |> ignore
)

// --------------------------------------------------------------------------------------
// Release Scripts

Target "ReleaseDocs" (fun _ ->
    let tempDocsDir = "temp/gh-pages"
    CleanDir tempDocsDir
    Repository.cloneSingleBranch "" cloneUrl "gh-pages" tempDocsDir

    fullclean tempDocsDir
    CopyRecursive "docs/output" tempDocsDir true |> tracefn "%A"
    StageAll tempDocsDir
    Git.Commit.Commit tempDocsDir (sprintf "[skip ci] Update generated documentation for version %s" release.NugetVersion)
    Branches.push tempDocsDir
)

#load "paket-files/fsharp/FAKE/modules/Octokit/Octokit.fsx"
open Octokit

#r @"packages/Newtonsoft.Json/lib/net40/Newtonsoft.Json.dll"
#r @"packages/Selenium.WebDriver/lib/net40/WebDriver.dll"
#r @"packages/Selenium.Support/lib/net40/WebDriver.Support.dll"
#r @"packages/canopy/lib/canopy.dll"
#r @"packages/SizSelCsZzz/lib/SizSelCsZzz.dll"
open canopy
open runner
open System


Target "UploadToGallery" (fun _ ->
    start chrome

    let vsixGuid = "ce104917-e8b3-4365-9490-8432c6e75c36"
    let galleryUrl = sprintf "https://visualstudiogallery.msdn.microsoft.com/%s/edit?newSession=True" vsixGuid

    let username,password =
        let lines = File.ReadAllLines("gallerycredentials.txt")
        lines.[0],lines.[1]

    // log in to msdn
    url galleryUrl    
    "#i0116" << username
    "#i0118" << password

    click "#idSIButton9"

    // start a new upload session - via hacky form link
    js (sprintf "$('form[action=\"/%s/edit/changeContributionUpload\"]').submit();" vsixGuid) |> ignore

    // select "upload the vsix"    
    let fi = System.IO.FileInfo("bin/Paket.VisualStudio.vsix")
    
    ".uploadFileInput" << fi.FullName 
    click "#setContributionTypeButton"
    
    click "#uploadButton"

    quit()
)


Target "Release" (fun _ ->
    StageAll ""
    Git.Commit.Commit "" (sprintf "Bump version to %s" release.NugetVersion)
    Branches.push ""

    Branches.tag "" release.NugetVersion
    Branches.pushTag "" "origin" release.NugetVersion

    // release on github
    createClient (getBuildParamOrDefault "github-user" "") (getBuildParamOrDefault "github-pw" "")
    |> createDraft gitOwner gitName release.NugetVersion (release.SemVer.PreRelease <> None) release.Notes 
    |> uploadFile "./bin/Paket.VisualStudio.vsix"
    |> releaseDraft
    |> Async.RunSynchronously
)

// --------------------------------------------------------------------------------------
// Run main targets by default. Invoke 'build <Target>' to override

Target "Default" DoNothing

"Clean"
  =?> ("BuildVersion", isAppVeyorBuild)
  ==> "AssemblyInfo"
  ==> "Build"
  ==> "CleanVSIX"
  ==> "Default"
  ==> "CleanDocs"
  ==> "GenerateDocs"
  ==> "ReleaseDocs"
  ==> "UploadToGallery"
  ==> "Release"

RunTargetOrDefault "Default"
