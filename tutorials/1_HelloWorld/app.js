(function(global){var String__Format$, Program__main$, Extensions__logFormat$, DateTime__get_Now$, DateTime__createUnsafe$, Array__BoxedLength$;
Array__BoxedLength$ = (function (xs) {
    return xs.length;;
});
DateTime__createUnsafe$ = (function (value,kind) {
    var date = value == null ? new Date() : new Date(value);
    if (isNaN(date)) { throw "The string was not recognized as a valid DateTime." }
    date.kind = kind;
    return date;
});
DateTime__get_Now$ = (function (unitVar0) {
    return DateTime__createUnsafe$(null, 2);
});
Extensions__logFormat$ = (function (str,args) {
    if ((Array__BoxedLength$(args) > 0)) {
      return console.log(String__Format$(str, args));
    }
    else {
      return console.log(str);
    };
});
Program__main$ = (function (unitVar0) {
    ((global.console).log("Hello JS!"));
    Extensions__logFormat$("Hello {0} at {1:d} {1:t}!", [".NET", DateTime__get_Now$()]);
    Extensions__logFormat$("Debug message", []);
    var h1 = (((global.document).getElementsByTagName("h1"))[0]);
    (h1.textContent) = "Hello World!";
    null;
    var x = 7;
    var _x = 5;
    return (function (y) {
      return (x / y);
    })((_x + 4));
});
String__Format$ = (function (s,args) {
    return s.replace(/\{(\d+)(,-?\d+)?(?:\:(.+?))?\}/g, function(match, number, alignment, format) {
        var rep = match;
        if (args[number] !== undefined) {
            rep = args[number];
            if (format !== undefined) {
                if (typeof rep === 'number') {            
                    switch (format.substring(0,1)) {
                        case "f": case "F": return format.length > 1 ? rep.toFixed(format.substring(1)) : rep.toFixed(2);
                        case "g": case "G": return format.length > 1 ? rep.toPrecision(format.substring(1)) : rep.toPrecision();
                        case "e": case "E": return format.length > 1 ? rep.toExponential(format.substring(1)) : rep.toExponential();
                        case "p": case "P": return (format.length > 1 ? (rep * 100).toFixed(format.substring(1)) : (rep * 100).toFixed(2)) + " %";
                    }                
                }
                else if (rep instanceof Date) {
                    if (format.length === 1) {
                        switch (format) {
                            case "D": return rep.toDateString();
                            case "T": return rep.toLocaleTimeString();
                            case "d": return rep.toLocaleDateString();
                            case "t": return rep.toLocaleTimeString().replace(/:\d\d(?!:)/, '');
                        }        
                    }
                    return format.replace(/(\w)\1*/g, function (match2) {
                        var rep2 = match2;
                        switch (match2.substring(0,1)) {
                            case "y": rep2 = match2.length < 4 ? rep.getFullYear() % 100 : rep.getFullYear(); break;
                            case "h": rep2 = rep.getHours() > 12 ? rep.getHours() % 12 : rep.getHours();      break;
                            case "M": rep2 = rep.getMonth() + 1; break;
                            case "d": rep2 = rep.getDate();      break;
                            case "H": rep2 = rep.getHours();     break;
                            case "m": rep2 = rep.getMinutes();   break;
                            case "s": rep2 = rep.getSeconds();   break;
                        }
                        if (rep2 !== match2 && rep2 < 10 && match2.length > 1) { rep2 = "0" + rep2; }
                        return rep2;
                    })                
                }
            }
        }
        return rep;
    });
});
Program__main$()}(typeof window!=='undefined'?window:global));