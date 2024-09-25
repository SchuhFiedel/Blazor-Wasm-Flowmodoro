
function callDotNetFunctionFromJS(functionName) {
    DotNet.invokeMethod('Flowmodoro.Client', functionName);
}

window.onbeforeunload = function() {
    callDotNetFunctionFromJS('PageAboutToBeReloaded');
    return undefined;
}