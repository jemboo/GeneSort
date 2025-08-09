namespace GeneSort.Core


type Uf4GenRates =
    {
        order:int;
        seedOpsGenRates: OpsGenRates;
        opsGenRatesList: OpsGenRates list
    }

module Uf4GenRates =

    let makeUniform (order:int) : Uf4GenRates =

        if order < 4 then
            failwith "TwoOrbitUfStep order must be at least 4"
        if order % 2 <> 0 then
            failwith "TwoOrbitUfStep order must be even"
        let genRatesListLength = MathUtils.exactLog2 (order / 4)
        let genRatesList = List.init genRatesListLength (fun _ -> OpsGenRates.createUniform())
        { 
            Uf4GenRates.order = order;
            seedOpsGenRates = OpsGenRates.createUniform();
            opsGenRatesList = genRatesList; 
        }


    let biasTowards (order:int) (opsGenMode:OpsGenMode) (biasAmt:float) 
                : Uf4GenRates =

        let genRatesBaseListLength = MathUtils.exactLog2 (order / 4)

        if (genRatesBaseListLength = 0)  then
            {
                Uf4GenRates.order = order;
                seedOpsGenRates = OpsGenRates.createUniform();
                opsGenRatesList = []
            }
        else
            let genRatesBaseList =
                if (genRatesBaseListLength = 1) 
                then []
                else List.init (genRatesBaseListLength - 1) (fun _ -> OpsGenRates.createUniform())

            let lastGenRates = OpsGenRates.createBiased(opsGenMode, biasAmt)

            let genRatesListWithLast = genRatesBaseList @ [lastGenRates]
            { 
                Uf4GenRates.order = order;
                seedOpsGenRates = OpsGenRates.createUniform();
                opsGenRatesList = genRatesListWithLast; 
            }


type Uf6GenRates =
    {
        order:int;
        seedGenRatesUf6: Seed6GenRates;
        opsGenRatesList: OpsGenRates list
    }

module Uf6GenRates =

    let makeUniform (order:int) =
        let genRatesListLength = MathUtils.exactLog2 (order / 6)
        let genRatesList = List.init genRatesListLength (fun _  -> OpsGenRates.createUniform())
        { Uf6GenRates.order = order;
          seedGenRatesUf6 = Seed6GenRates.createUniform();
          opsGenRatesList = genRatesList; }
           

type Uf4MutationRates =
    {
        order:int;
        seedOpsTransitionRates: OpsTransitionRates;
        twoOrbitPairOpsTransitionRates: OpsTransitionRates list
    }

module Uf4MutationRates =

    let makeUniform (order:int) (perm_RsMutationRate:float) (twoOrbitMutationRate:float) =
        let mutRatesListLength = MathUtils.exactLog2 (order / 4)
        let mutRatesList = List.init mutRatesListLength (fun _ -> OpsTransitionRates.createUniform(twoOrbitMutationRate))
        { Uf4MutationRates.order = order;
          seedOpsTransitionRates = OpsTransitionRates.createUniform(perm_RsMutationRate);
          twoOrbitPairOpsTransitionRates = mutRatesList; }

    let biasTowards (order:int) (perm_RsMutationRate:float) (twoOrbitType:TwoOrbitType)  (baseAmt:float) (biasAmt:float) =
        let mutRatesBaseListLength = MathUtils.exactLog2 (order / 4) - 1
        let mutRatesBaseList = List.init mutRatesBaseListLength (fun _ -> OpsTransitionRates.createUniform(baseAmt))
        let lastGenRates = OpsTransitionRates.createBiased twoOrbitType baseAmt biasAmt
        let genRatesListWithLast = mutRatesBaseList @ [lastGenRates]
        { Uf4MutationRates.order = order;
          seedOpsTransitionRates = OpsTransitionRates.createUniform(perm_RsMutationRate);
          twoOrbitPairOpsTransitionRates = genRatesListWithLast; }

 
type Uf6MutationRates =
    {
        order:int;
        seed6TransitionRates: Seed6TransitionRates;
        opsTransitionRates: OpsTransitionRates list
    }


module Uf6MutationRates =

    let makeUniform (order:int) (seed6MutationRates:float) (twoOrbitMutationRate:float) 
         : Uf6MutationRates   =
        let mutRatesListLength = MathUtils.exactLog2 (order / 6)
        let mutRatesList = List.init mutRatesListLength (fun _ -> OpsTransitionRates.createUniform(twoOrbitMutationRate))
        { Uf6MutationRates.order = order;
          seed6TransitionRates = Seed6TransitionRates.createUniform(seed6MutationRates);
          opsTransitionRates = mutRatesList; }


    let biasTowards (order:int) (seed6MutationRates:float) (twoOrbitType:TwoOrbitType) (baseAmt:float) (biasAmt:float) 
         : Uf6MutationRates =
        let mutRatesBaseListLength = MathUtils.exactLog2 (order / 6) - 1
        let mutRatesBaseList = List.init mutRatesBaseListLength (fun _ -> OpsTransitionRates.createUniform(baseAmt))
        let lastGenRates = OpsTransitionRates.createBiased twoOrbitType baseAmt biasAmt
        let genRatesListWithLast = mutRatesBaseList @ [lastGenRates]
        { Uf6MutationRates.order = order;
          seed6TransitionRates = Seed6TransitionRates.createUniform(seed6MutationRates);
          opsTransitionRates = genRatesListWithLast; }