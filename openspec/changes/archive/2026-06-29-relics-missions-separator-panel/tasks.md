## 1. Restructure MainWindow.axaml grid

> Post-apply correction: the separator panel cannot occupy its own outer grid row
> (`Auto,*,Auto,4,*`) — the GridSplitter would then resize the panel instead of the
> relic-cards scroll. Final layout uses `Auto,*,4,*` with the panel pinned to the
> bottom of the relic-cards section via a nested `*,Auto` grid in Row 1.

- [x] 1.1 Set `RelicsMissionsGrid` `RowDefinitions` to `Auto,*,4,*`
- [x] 1.2 Wrap the relic-cards `ScrollViewer` and the separator panel in a nested `Grid` (`RowDefinitions="*,Auto"`) at outer `Grid.Row="1"`
- [x] 1.3 Place the `GridSplitter` at outer `Grid.Row="2"` and the missions `ScrollViewer` at outer `Grid.Row="3"`

## 2. Add missions separator panel (bottom of relic-cards section)

- [x] 2.1 Add a `Border` at nested `Grid.Row="1"` styled as a compact divider bar (`Background="#1A808080"`, `Margin="8,4,8,0"`, `Padding="12,2"`, `CornerRadius="4,4,0,0"` so the bottom corners are square and the height matches the "Vaulted" separator bar)
- [x] 2.2 Inside the Border add a horizontal `StackPanel` containing: "Missions:" `TextBlock`, then a `StackPanel` with the "Show exact drop chances" `CheckBox` and its label, bound to `WishlistFarming.ShowExactDropChances`

## 3. Clean up top control panel (Row 0)

- [x] 3.1 Remove the thick vertical divider `Border` (the `Width="2"` separator) from the top control panel `StackPanel`
- [x] 3.2 Remove the "Missions:" `TextBlock` from the top control panel
- [x] 3.3 Remove the "Show exact drop chances" `CheckBox` and its label `StackPanel` from the top control panel

## 4. Card chrome adjustments (post-apply follow-up)

- [x] 4.1 Remove the `<Separator>` between the relic-name header and the rewards body in the relic-card template
- [x] 4.2 In the mission-card template, keep the separator after the title/chance header and add a second `<Separator>` after the node-names line
- [x] 4.3 Change the mission-card node-names `TextBlock` from italic/dimmed (`FontStyle="Italic"`, `Opacity="0.7"`) to regular font

## 5. Verify

- [x] 5.1 Build the project and confirm no AXAML or binding errors
- [x] 5.2 Visually confirm the top panel shows only "Relics: [ ] Show vaulted [ ] Show all rewards"
- [x] 5.3 Visually confirm the separator panel shows "Missions: [ ] Show exact drop chances", is a compact divider bar matching the "Vaulted" bar height, and has squared bottom corners
- [x] 5.4 Confirm the GridSplitter resizes the relic-cards and missions scroll areas (not the separator panel)
- [x] 5.5 Visually confirm relic cards have no line under the relic name, and mission cards have a divider after both the header and the node names with node names in regular font

