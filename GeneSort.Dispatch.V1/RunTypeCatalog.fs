namespace GeneSort.Dispatch.V1

/// Explicit module registrations; each module owns its examples and catalog selections.
module RunTypeCatalog =
    let all =
        [|
            SortableTests.Merge.runType
            SortableTests.Prefix.runType
            SorterEval.Merge.runType
            SorterEval.Prefix.runType
            SorterEval.Standard.runType
            SorterMutate.Merge.runType
            SorterMutate.Prefix.runType
            SorterMutate.Standard.runType
            SorterSgd.p24.Msrs3a.OrthoPara.OrthoPara32.runType
            SorterSgd.p24.Msrs3a.PoolModComp.PoolSelSz128.runType
            SorterSgd.p24.Msrs3a.PoolModComp.PoolsSelSzTest_2_vs_256.runType
            SorterSgd.p24.Msrs3a.PoolModComp.Sz_2048_Of_4096.runType
            SorterSgd.p24.Msrs3b.OrthoPara.OrthoPara32.runType
            SorterSgd.p24.Mssi3b.OrthoPara.OrthoPara128.runType
            SorterSgd.p24.Mssi3b.OrthoPara.OrthoPara64.runType
            SorterSgd.p24.Msuf63b.MutationRate.VarModR_32.runType
            SorterSgd.p32.Msrs4a.MutationRate.MaxModR_32.runType
            SorterSgd.p32.Msrs4a.MutationRate.MaxModR_64.runType
            SorterSgd.p32.Msrs4a.MutationRate.VarModR_32.runType
            SorterSgd.p32.Msrs4a.MutationRate.VarModR_64.runType
            SorterSgd.p32.Msuf4a.SimpleR.Soss.runType
            SorterSgd.p32.Msuf4a.SimpleR.Standard.runType
            SorterSgd.p32.Msuf4a.SimpleS.Soss.runType
            SorterSgd.p32.Msuf4a.SimpleS.Standard.runType
            SorterSgd.p32.Msuf4a.Soss_condense.Pool_X.runType
            SorterSgd.p32.Msuf4a.Soss.Pool_32.runType
            SorterSgd.p32.Msuf4a.Soss.Pool_512.runType
            SorterSgd.p32.Msuf4a.Standard_append.Pool_128.runType
            SorterSgd.p32.Msuf4a.Standard_append.Pool_512.runType
            SorterSgd.p32.Msuf4a.Standard_append.runType
            SorterSgd.p32.Msuf4a.Standard_condense.Pool_32_condense.runType
            SorterSgd.p32.Msuf4a.Standard.Pool_32.runType
            SorterSgd.p32.Msuf4a.Standard.Pool_512.runType
            SorterSgd.p32.Test.OnePool.Continuation.runType
            SorterSgd.p32.Test.OnePool.Controlled.runType
            SorterSgd.p32.Test.OnePool.MixedFamilyChain.runType
            SorterSgd.p32.Test.OnePool.runType
            SorterSgd.p32.Test.OnePool.SameFamilyChain.runType
            SorterSgd.p32.Test.OnePool.SimpleR.runType
            SorterSgd.p32.Test.OnePool.SimpleS.runType
        |]

