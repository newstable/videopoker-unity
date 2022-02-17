mergeInto(LibraryManager.library, {
  GameReady: function (msg) {
    window.dispatchReactUnityEvent(
      "GameReady",
      Pointer_stringify(msg)
    );
  },
});