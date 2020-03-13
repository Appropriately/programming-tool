var unityInstance;
var modal;

window.onload = function(){
    modal = document.getElementById("game");

    document.querySelectorAll('.game-button').forEach(button => {
        button.addEventListener('click', event => {
            modal.style.display = "flex";
            unityInstance = UnityLoader.instantiate("unityContainer", "Build/Game.json", {onProgress: UnityProgress});
        })
    });

    document.querySelectorAll('.close-button').forEach(button => {
        button.addEventListener('click', event => close())
    });

    window.onclick = function(event) {
        if (event.target == modal) {
            close();
        }
    }
};

function close() {
    modal.style.display = "none";
    unityInstance.Quit();
    unityInstance = null;
}