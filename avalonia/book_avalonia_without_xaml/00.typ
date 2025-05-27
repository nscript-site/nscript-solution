
#set page(
    paper:"iso-b5",
    margin: (
        top:25.5mm,
        bottom:28.4mm,
        left:28.7mm,
        right:28.7mm
    ),
    numbering:"1",
    number-align:center,
)


#set heading(
    numbering: none
)

#set figure(
    supplement: "图",
)


#set par(justify: true, spacing: 0.6cm, first-line-indent: {0em}, leading: 0.8em)
#set text(10pt)

#show heading.where(
  level: 1
): it => {
  set align(center)
  set text(20pt, weight: "bold")
  block()
  it
  block(below: 0.5em)
}

#show heading.where(
  level: 2
): it => {
  set align(left)
  set text(15pt, weight: "bold")
  block()
  it
  block(below: 0.5em)
}

#align(center, text(18pt)[

\

\

Avalonia without xaml

(不用 xaml 开发 Avalonia 应用)

\

Avalonia without xaml


nscript\@zhihu

\

])

#align(center, text(18pt)[

])

#pagebreak()
#outline()
#pagebreak()


= 前言

= 第一个 Avalonia 应用

= 列表应用

= 布局

= 样式

= 动画

= 桌面应用

= Android 应用

= iOS 应用

= wasm 应用

= 使用 xaml