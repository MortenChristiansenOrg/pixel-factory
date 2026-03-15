# Agents Notes

## WPF Theme ResourceDictionary gotchas (.NET 10)

- **StaticResource across sibling merged dictionaries**: When control style files are merged into a parent `Theme.xaml`, `StaticResource` in one file CANNOT resolve resources from a sibling file at BAML load time. Fix: each style file must merge `Colors.xaml` in its own `MergedDictionaries`.
- **Implicit TextBlock styles**: Never define an implicit style (no x:Key) for `TextBlock` at the application level. It causes `UnsetValue` crashes inside control templates that use TextBlock/AccessText internally. Instead, set `Foreground` on the Window and rely on property inheritance.
- **OverridesDefaultStyle + trigger-only Template**: If a style sets `OverridesDefaultStyle=True` but only sets `Template` via triggers, there's a window where no template exists. Always provide a default `Template` setter alongside trigger overrides.
