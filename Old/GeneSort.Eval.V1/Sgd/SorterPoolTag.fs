namespace GeneSort.Eval.V1.Sgd

open System

/// Represents the dimension bounds (extents) of a finite toroidal lattice.
type latticeBounds =
    | Dim1 of sizeX: int
    | Dim2 of sizeX: int * sizeY: int
    | Dim3 of sizeX: int * sizeY: int * sizeZ: int

/// Coordinate on a 1D, 2D, or 3D toroidal lattice.
type sorterPoolTag =
    | D1 of int
    | D2 of int * int
    | D3 of int * int * int

module SorterPoolTag =

    // --- Format / Serialization Helpers ---

    let toString (spt: sorterPoolTag) : string =
        match spt with
        | D1 i -> sprintf "D1_%d" i
        | D2 (i1, i2) -> sprintf "D2_%d_%d" i1 i2
        | D3 (i1, i2, i3) -> sprintf "D3_%d_%d_%d" i1 i2 i3

    let private (|Int32|_|) (str: string) =
        match Int32.TryParse str with
        | true, intValue -> Some intValue
        | false, _ -> None

    let fromString (what: string) : sorterPoolTag =
        if String.IsNullOrWhiteSpace what then
            invalidArg (nameof what) "Tag string cannot be null or empty."

        match what.Split('_') with
        | [| "D1"; Int32 i |] -> D1 i
        | [| "D2"; Int32 i1; Int32 i2 |] -> D2 (i1, i2)
        | [| "D3"; Int32 i1; Int32 i2; Int32 i3 |] -> D3 (i1, i2, i3)
        | _ -> invalidArg (nameof what) (sprintf "Invalid sorterPoolTag format: '%s'" what)


    // --- Toroidal Mathematics & Grid Helpers ---

    /// Mathematical floored modulo ensuring non-negative index in range [0, bounds - 1].
    let inline private modWrap (value: int) (bound: int) : int =
        if bound <= 0 then 
            invalidArg (nameof bound) "Lattice dimension size must be positive."
        let r = value % bound
        if r < 0 then r + bound else r

    /// Normalizes/wraps a coordinate tag to fit strictly within bounded toroidal extents.
    let wrap (bounds: latticeBounds) (spt: sorterPoolTag) : sorterPoolTag =
        match bounds, spt with
        | Dim1 sx, D1 x -> 
            D1 (modWrap x sx)
        | Dim2 (sx, sy), D2 (x, y) -> 
            D2 (modWrap x sx, modWrap y sy)
        | Dim3 (sx, sy, sz), D3 (x, y, z) -> 
            D3 (modWrap x sx, modWrap y sy, modWrap z sz)
        | _ -> 
            invalidOp "Mismatched dimensions between lattice bounds and sorterPoolTag."

    /// Moves a coordinate tag by a delta offset and wraps around toroidal boundaries.
    let move (delta: sorterPoolTag) (bounds: latticeBounds) (spt: sorterPoolTag) : sorterPoolTag =
        match spt, delta with
        | D1 x, D1 dx -> 
            wrap bounds (D1 (x + dx))
        | D2 (x, y), D2 (dx, dy) -> 
            wrap bounds (D2 (x + dx, y + dy))
        | D3 (x, y, z), D3 (dx, dy, dz) -> 
            wrap bounds (D3 (x + dx, y + dy, z + dz))
        | _ -> 
            invalidOp "Mismatched dimensions between tag and offset delta."

    /// Generates von Neumann (orthogonal) immediate 1-step neighbors on a toroidal lattice.
    let getNeighbors (bounds: latticeBounds) (spt: sorterPoolTag) : sorterPoolTag list =
        let wrappedTag = wrap bounds spt
        match bounds, wrappedTag with
        | Dim1 sx, D1 x ->
            [ D1 (modWrap (x - 1) sx)
              D1 (modWrap (x + 1) sx) ]

        | Dim2 (sx, sy), D2 (x, y) ->
            [ D2 (modWrap (x - 1) sx, y)
              D2 (modWrap (x + 1) sx, y)
              D2 (x, modWrap (y - 1) sy)
              D2 (x, modWrap (y + 1) sy) ]

        | Dim3 (sx, sy, sz), D3 (x, y, z) ->
            [ D3 (modWrap (x - 1) sx, y, z)
              D3 (modWrap (x + 1) sx, y, z)
              D3 (x, modWrap (y - 1) sy, z)
              D3 (x, modWrap (y + 1) sy, z)
              D3 (x, y, modWrap (z - 1) sz)
              D3 (x, y, modWrap (z + 1) sz) ]

        | _ -> 
            invalidOp "Mismatched dimensions between lattice bounds and sorterPoolTag."

    /// Calculates toroidal shortest-path Manhattan distance between two tags on a lattice.
    let distance (bounds: latticeBounds) (tagA: sorterPoolTag) (tagB: sorterPoolTag) : int =
        let toroidalDist (a: int) (b: int) (size: int) =
            let d = Math.Abs(a - b) % size
            Math.Min(d, size - d)

        match bounds, wrap bounds tagA, wrap bounds tagB with
        | Dim1 sx, D1 x1, D1 x2 ->
            toroidalDist x1 x2 sx

        | Dim2 (sx, sy), D2 (x1, y1), D2 (x2, y2) ->
            toroidalDist x1 x2 sx + toroidalDist y1 y2 sy

        | Dim3 (sx, sy, sz), D3 (x1, y1, z1), D3 (x2, y2, z2) ->
            toroidalDist x1 x2 sx + toroidalDist y1 y2 sy + toroidalDist z1 z2 sz

        | _ -> 
            invalidOp "Mismatched tag and lattice dimensions for distance calculation."


    /// Maps a 1D flat integer index to a valid bounded sorterPoolTag using row-major ordering.
    /// Performs toroidal wrapping automatically if index is out of bounds or negative.
    let fromIndex (bounds: latticeBounds) (index: int) : sorterPoolTag =
        match bounds with
        | Dim1 sx ->
            let x = modWrap index sx
            D1 x

        | Dim2 (sx, sy) ->
            let total = sx * sy
            let wrapped = modWrap index total
            let x = wrapped % sx
            let y = wrapped / sx
            D2 (x, y)

        | Dim3 (sx, sy, sz) ->
            let total = sx * sy * sz
            let wrapped = modWrap index total
            let x = wrapped % sx
            let y = (wrapped / sx) % sy
            let z = wrapped / (sx * sy)
            D3 (x, y, z)

    /// Maps a sorterPoolTag back to its 1D flat index (row-major order).
    let toIndex (bounds: latticeBounds) (spt: sorterPoolTag) : int =
        match bounds, wrap bounds spt with
        | Dim1 sx, D1 x -> 
            x

        | Dim2 (sx, sy), D2 (x, y) -> 
            x + y * sx

        | Dim3 (sx, sy, sz), D3 (x, y, z) -> 
            x + y * sx + z * (sx * sy)

        | _ -> 
            invalidOp "Mismatched dimensions between lattice bounds and sorterPoolTag."

    /// Returns the total volume / cell count of a given lattice.
    let totalCells (bounds: latticeBounds) : int =
        match bounds with
        | Dim1 sx -> sx
        | Dim2 (sx, sy) -> sx * sy
        | Dim3 (sx, sy, sz) -> sx * sy * sz



module LatticeBounds =

    let toString (bounds: latticeBounds) : string =
        match bounds with
        | Dim1 sx -> sprintf "Dim1_%d" sx
        | Dim2 (sx, sy) -> sprintf "Dim2_%d_%d" sx sy
        | Dim3 (sx, sy, sz) -> sprintf "Dim3_%d_%d_%d" sx sy sz

    let private (|Int32|_|) (str: string) =
        match Int32.TryParse str with
        | true, intValue -> Some intValue
        | false, _ -> None

    let fromString (what: string) : latticeBounds =
        if String.IsNullOrWhiteSpace what then
            invalidArg (nameof what) "Lattice bounds string cannot be null or empty."

        match what.Split('_') with
        | [| "Dim1"; Int32 sx |] when sx > 0 -> 
            Dim1 sx
        | [| "Dim2"; Int32 sx; Int32 sy |] when sx > 0 && sy > 0 -> 
            Dim2 (sx, sy)
        | [| "Dim3"; Int32 sx; Int32 sy; Int32 sz |] when sx > 0 && sy > 0 && sz > 0 -> 
            Dim3 (sx, sy, sz)
        | _ -> 
            invalidArg (nameof what) (sprintf "Invalid latticeBounds format or non-positive dimension bounds: '%s'" what)
