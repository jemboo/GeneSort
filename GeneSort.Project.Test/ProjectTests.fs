namespace GeneSort.Project.Test

open FSharp.UMX
open Xunit
open GeneSort.Project.V1

type ProjectTests() =
    [<Fact>]
    member _.``Run parameters expand execution spans across replicas`` () =
        let configured = SimpleRun.create {
            ProjectName = UMX.tag "TestProject"
            DatabaseName = UMX.tag "TestDatabase"
            RunName = UMX.tag "TestRun"
            Description = "Fixed parameter expansion fixture"
            AllowOverwrite = false
            Setup = {
                FunctionName = "setup"
                ParameterSpans = ["SetupValue", ["seed"]]
                QueryCatalogName = "setup-query"
                ParametersEnhancerName = "setup-enhancer"
            }
            Execution = {
                FunctionName = "execute"
                ParameterSpans = ["A", ["1"; "2"]; "B", ["x"; "y"]]
                QueryCatalogName = "run-query"
                ParametersEnhancerName = "run-enhancer"
            }
        }
        let parameters = SimpleRun.makeRunParameters 0<replNumber> 2<replNumber> id configured
        Assert.Equal(8, parameters.Length)
        let combinations = parameters |> Array.map (fun rp -> rp.GetRepl().Value, rp.ParamMap.["A"], rp.ParamMap.["B"]) |> Set.ofArray
        let expected = set [for replica in [0<replNumber>; 1<replNumber>] do
                               for a in ["1"; "2"] do
                                   for b in ["x"; "y"] do yield replica, a, b]
        Assert.Equal<Set<int<replNumber> * string * string>>(expected, combinations)
        Assert.All(parameters, fun rp -> Assert.False(rp.ParamMap.ContainsKey "SetupValue"))
