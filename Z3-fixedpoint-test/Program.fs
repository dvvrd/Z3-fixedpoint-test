open Microsoft
open System

[<EntryPoint>]
let main argv = 
    let cfg =
        let list = [ ("AUTO_CONFIG", "true" ) ]
        System.Linq.Enumerable.ToDictionary(list, fst, snd)
    use ctx = new Z3.Context(cfg)
    let fp = ctx.MkFixedpoint()

    let n = ctx.MkBound(1u, ctx.MkIntSort()) :?> Z3.ArithExpr
    let tmp1 = ctx.MkBound(2u, ctx.MkIntSort()) :?> Z3.ArithExpr
    let tmp2 = ctx.MkBound(3u, ctx.MkIntSort()) :?> Z3.ArithExpr
    let query1rel = ctx.MkFuncDecl("query1", [| |], ctx.MkBoolSort() :> Z3.Sort)
    let query2rel = ctx.MkFuncDecl("query2", [| |], ctx.MkBoolSort() :> Z3.Sort)
    let fib = ctx.MkFuncDecl("Fib1!!1", [| ctx.MkIntSort() :> Z3.Sort; ctx.MkIntSort() :> Z3.Sort |], ctx.MkBoolSort() :> Z3.Sort)
    fp.RegisterRelation(query1rel)
    fp.RegisterRelation(query2rel)
    fp.RegisterRelation(fib)

    let t = ctx.MkTrue()
    let zero = ctx.MkInt(0)
    let one = ctx.MkInt(1)
    let two = ctx.MkInt(2)

    let ruleFib1 = ctx.MkImplies(ctx.MkLt(n, two), fib.Apply(n, one) :?> Z3.BoolExpr)
    let ruleFib2 = ctx.MkImplies(ctx.MkAnd(
                                     ctx.MkGe(n, two),
                                     fib.Apply(ctx.MkSub(n, one), tmp1) :?> Z3.BoolExpr,
                                     fib.Apply(ctx.MkSub(n, two), tmp2) :?> Z3.BoolExpr),
                                 fib.Apply(n, ctx.MkAdd(tmp1, tmp2)) :?> Z3.BoolExpr)

    let target1 = query1rel.Apply() :?> Z3.BoolExpr
    let target2 = query2rel.Apply() :?> Z3.BoolExpr
    let query1 = ctx.MkImplies(ctx.MkLt(n, two), target1)
    let query2 = ctx.MkImplies(ctx.MkAnd(fib.Apply(n, tmp1) :?> Z3.BoolExpr, ctx.MkLe(tmp1, zero)), target2)

    fp.AddRule(ruleFib1)
    fp.AddRule(ruleFib2)
    fp.AddRule(query1)
    fp.AddRule(query2)

    Console.WriteLine(fp.ToString())

    let query (target : Z3.BoolExpr) =
        Console.WriteLine("-----------------------")
        let status = fp.Query(target)
        Console.WriteLine(status.ToString())
        if status.Equals Z3.Status.UNKNOWN then
            Console.WriteLine(fp.GetReasonUnknown())
        else
            Console.WriteLine(fp.GetAnswer())

        Console.WriteLine("-----------------------")

    query target1
    query target2

    Console.ReadKey() |> ignore
    0
