namespace GeneSort.Core


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