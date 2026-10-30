namespace GeneSort.FileDb.V1

open System.IO
open GeneSort.Project.V1.Editing

/// Map logical project identities to nested folders, with the database included once.
module DatabasePaths =
    let relativeFolder (project: string) (database: string) =
        for field, value in ["ProjectName", project; "DatabaseName", database] do
            if (RunIdentity.validate field value).Length > 0 then
                invalidArg field "Invalid storage identity."
        let parts = project.Split('.')
        if parts |> Array.exists (fun part -> (RunIdentity.validate "ProjectName" part).Length > 0) then
            invalidArg "project" "Project hierarchy must contain valid folder names."
        let folders =
            if parts.Length > 1 && Array.last parts = database then parts
            else Array.append parts [|database|]
        Path.Combine folders

    let dataRoot projectsFolder project database =
        Path.Combine(projectsFolder, relativeFolder project database, "Data")
