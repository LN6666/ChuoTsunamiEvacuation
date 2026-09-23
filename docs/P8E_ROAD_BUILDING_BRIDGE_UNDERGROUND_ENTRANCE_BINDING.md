# P8-E Road / Building / Bridge / Underground / Entrance Binding

## Road

Road binding is `plateau_metadata` from local PLATEAU transport metadata plus P8-C road hazard status. This supports road restricted-proxy status only. It does not validate official evacuation routes or final road passability.

## Building

Building binding is `plateau_metadata` plus P8-D damage-proxy status. It supports low-floor warning, building damaged proxy, and collapse visual proxy labels. It is not an engineering damage prediction.

## Bridge

Bridge binding is `plateau_metadata` where local bridge GML exists. P8-D bridge restricted-proxy status can be applied as visual/status data only.

## Underground

Underground binding remains `data_only`. No reliable local underground scene semantics were found. P9 must not assume real subway entrance geometry.

## Entrance

Entrance binding remains `proxy_marker`. P8-D can emit `entrance_blocked_proxy`, but P8-E does not create real entrance scene objects. Future vertical evacuation should use entrance / safe-floor / evacuation-complete proxy, not real indoor scenes.
