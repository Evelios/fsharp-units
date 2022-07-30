﻿namespace Units

open System

// ---- Unit Systems -----------------------------------------------------------

/// A special units type representing 'no units'. A `Quantity Int Unitless`
/// value is interchangeable with a simple `Int`, and a `Quantity Float Unitless`
/// value is interchangeable with a simple `Float`.
/// A generic number that doesn't undergo any type mutation.
/// Eg.
///     Unitless: Unitless 1. * Unitless 1. = Unitless 1.
///     Meters  : Meters 1. * Meters 1. = (Meters Squared) 1.
type Unitless = Unitless

type Pixels = Pixels

type Meters = Meters

type Percentage = Percentage

type Radians = Radians

// ---- Unit Ratios ------------------------------------------------------------

/// Represents a units type that is the square of some other units type; for
/// example, `Meters` is one units type (the units type of a [`Length`](Length)) and
/// `Squared Meters` is another (the units type of an [`Area`](Area)). See the
/// [`squared`](#squared) and [`sqrt`](#sqrt) functions for examples of use.
/// This is a special case of the `Product` units type.
type Squared<'Units> = Product of 'Units * 'Units


/// Represents a units type that is the product of two other units types. This
/// is a more general form of `Squared` or `Cubed`. See [`product`](#product),
/// [`times`](#times), [`over`](#over) and [`over_`](#over_) for how it can be used.
type Product<'Unit1, 'Unit2> = Product of 'Unit1 * 'Unit2


/// Represents a units type that is the cube of some other units type; for
/// example, `Meters` is one units type (the units type of a [`Length`](Length)) and
/// `Cubed Meters` is another (the units type of an [`Volume`](Volume)). See the
/// [`cubed`](Quantity#cubed) and [`cbrt`](Quantity#cbrt) functions for examples of
/// use.
/// This is a special case of the `Product` units type.
type Cubed<'Units> = Cubed of Product<'Units, 'Units> * 'Units


/// Represents the units type of a rate or quotient such as a speed (`Rate
/// Meters Seconds`) or a pressure (`Rate Newtons SquareMeters`). See [Working with
/// rates](#working-with-rates) for details.
type Rate<'DependentUnits, 'IndependentUnits> = Rate of 'DependentUnits * 'IndependentUnits

// ---- Quantity Values --------------------------------------------------------

/// A `Quantity` is effectively a `number` (an `Int` or `Float`) tagged with a
/// `units` type. So a
///     Quantity Float Meters
/// is a `Float` number of `Meters` and a
///     Quantity Int Pixels
/// is an `Int` number of `Pixels`. When compiling with `elm make --optimize` the
/// `Quantity` wrapper type will be compiled away, so the runtime performance should
/// be comparable to using a raw `Float` or `Int`.
type Quantity<'Units>(quantity: float) =
    interface IComparable<Quantity<'Units>> with
        member this.CompareTo(percent) = this.Comparison(percent)

    interface IComparable with
        member this.CompareTo(obj) =
            match obj with
            | :? (Quantity<'Units>) as quantity -> this.Comparison(quantity)
            | _ -> failwith "incompatible comparison"



    member this.Value = quantity
    static member create(newQuantity: float) : Quantity<'a> = Quantity(newQuantity)

    member this.Comparison(other: Quantity<'Units>) =
        if this.Equals(other) then 0
        elif this.LessThan(other) then -1
        else 1

    member this.Equals(other: Quantity<'Units>) : bool = almostEqual this.Value other.Value

    member this.LessThan(other: Quantity<'Units>) = this.Value < other.Value

    override this.ToString() = string quantity

    override this.GetHashCode() =
        this.Value
        |> roundFloatTo Float.DigitPrecision
        |> hash

    override this.Equals(obj: obj) : bool =
        match obj with
        | :? (Quantity<'Units>) as other -> this.Equals(other)
        | _ -> false

    // Operators
    static member Abs(q: Quantity<'Units>) : Quantity<'Units> = Quantity<'Units> (abs q.Value)

    static member Min(lhs: Quantity<'Units>, rhs: Quantity<'Units>) : Quantity<'Units> =
        Quantity<'Units> (min lhs.Value rhs.Value)

    static member Max(lhs: Quantity<'Units>, rhs: Quantity<'Units>) : Quantity<'Units> =
        Quantity<'Units> (max lhs.Value rhs.Value)

    static member Sqrt(value: Quantity<'Units Squared>) : Quantity<'Units> =
        Quantity<'Units> (sqrt value.Value)

    static member Floor(value: Quantity<'Units>) : Quantity<'Units> =
        Quantity<'Units> (floor value.Value)

    static member Ceiling(q: Quantity<'Units>) : Quantity<'Units> = Quantity<'Units> (ceil q.Value)

    static member Round(q: Quantity<'Units>) : Quantity<'Units> = Quantity<'Units> (round q.Value)

    static member Truncate(q: Quantity<'Units>) : Quantity<'Units> =
        Quantity<'Units> (truncate q.Value)

    static member (+)(lhs: Quantity<'Units>, rhs: Quantity<'Units>) : Quantity<'Units> =
        Quantity<'Units> (lhs.Value + rhs.Value)

    static member (-)(lhs: Quantity<'Units>, rhs: Quantity<'Units>) : Quantity<'Units> =
        Quantity<'Units> (lhs.Value - rhs.Value)

    static member (~-)(q: Quantity<'Units>) : Quantity<'Units> = Quantity<'Units> (-q.Value)

    static member (*)(q: Quantity<'Units>, scale: float) : Quantity<'Units> =
        Quantity<'Units> (q.Value * scale)

    static member (*)(scale: float, q: Quantity<'Units>) : Quantity<'Units> =
        Quantity<'Units> (q.Value * scale)

    static member (*)(lhs: Quantity<'Units>, rhs: Quantity<'Units>) : Quantity<'Units Squared> =
        Quantity<'Units Squared> (lhs.Value * rhs.Value)

    static member (/)(q: Quantity<'Units>, scale: float) : Quantity<'Units> =
        Quantity<'Units> (q.Value / scale)

    static member (/)(q: Quantity<'Units>, scale: Quantity<'Units>) : float = q.Value / scale.Value

    static member (/)
        (
            dependent: Quantity<'Dependent>,
            independent: Quantity<'Independent>
        ) : Quantity<Rate<'Dependent, 'Independent>> =
        Quantity(dependent.Value / independent.Value)

    static member (%)(q: Quantity<'Units>, modulus: Quantity<'Units>) : Quantity<'Units> =
        Quantity(q.Value % modulus.Value)


// ---- Unit Space Aliases ------------------------------------------------------


/// A percentage value. The default range for percentages is 0 to 1 but can also be given in the range 0 to 100.
type Percent = Quantity<Percentage>

type Angle = Quantity<Radians>

type Length = Quantity<Meters>

type Area = Quantity<Meters Squared>


// ---- Interval ---------------------------------------------------------------


/// A finite, closed interval with a minimum and maximum number. This can
/// represent an interval of any type.
///
/// For example...
///     Interval float
///     Interval int
///     Interval Angle
[<CustomEquality>]
[<NoComparison>]
type Interval<'T when 'T: equality> =
    | Interval of 'T * 'T

    override this.Equals(obj: obj) : bool =
        match obj with
        | :? (Interval<'T>) as other ->
            match this, other with
            | Interval (thisStart, thisFinish), Interval (otherStart, otherFinish) ->
                thisStart = otherStart && thisFinish = otherFinish

        | _ -> false

    override this.GetHashCode() : int =
        match this with
        | Interval (start, finish) -> HashCode.Combine(start, finish)
