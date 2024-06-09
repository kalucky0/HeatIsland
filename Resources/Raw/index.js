function callCSharp(method, args) {
    window.location.href = 'cs://' + method + '?args=' + JSON.stringify(args.map(arg => arg.toString()));
}