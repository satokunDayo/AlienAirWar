<h1>Alien-Air-War</h1>

<p>
Unity と Blender による試製 3D フライトゲーム<br>
<b>MiG-25 vs UFO: A 3D Flight Combat Prototype built in 2 weeks</b>
</p>

<p>
本作で使用している 3D モデルおよびテクスチャは、すべて Blender を用いて自作したものであり、生成AIによるアセット生成は一切行っていません。  
AI はあくまで設計議論・デバッグ補助・コーディングサポートなどの目的でのみ利用しています。
</p>

<p>
All 3D models and textures used in this project were fully created by myself in Blender.  
No AI‑generated assets were used.  AI tools were utilized only for discussions, debugging, and coding support. 
</p>



<h2>概要 / Overview</h2>

<p>
本プロジェクトは、ゲーム開発未経験の状態から「二週間でフル 3D ゲームを形にする」という挑戦から生まれた空戦シューティングです。<br>
サマーインターンに間に合わせるために制作を開始しましたが、どうせ最初に作るなら、私が恋焦がれる 70 年代の無機質で機能美を追求し、西側諸国を恐怖させた MiG-25 を飛ばしたい──そんなこだわりも込めています。
</p>

<p>
This project began as a challenge to create a full 3D game in just two weeks, starting from a completely inexperienced state in game development.<br>
It was originally built to meet the deadline for a summer internship, but if I was going to create my first Unity-based game, I wanted to fly the MiG‑25 — the cold, functional, fear‑inducing icon of the 1970s that has always fascinated me.
</p>

<h2>技術的焦点 / Technical Focus</h2>

<h3>1. 描画最適化 / LOD System</h3>
<p>
広大な砂漠フィールドを維持しつつメモリ使用量を抑えるため、7×7 グリッドによる動的 LOD（Level of Detail）切り替えシステムを自作しました。<br>
カメラ距離に応じてメッシュ解像度を動的に制御し、参照ベースのモデルを用いることで低負荷かつ安定した描画を実現しています。
</p>

<p>
To maintain a vast desert field while minimizing memory usage, I implemented a custom 7×7 grid dynamic LOD system.  
It adjusts mesh resolution based on camera distance and uses reference-based models to ensure stable performance.
</p>

<h3>2. 物理ベースの誘導ミサイル / Physics‑Based Guided Missile</h3>
<p>
ミサイル挙動には個人的なこだわりを反映した物理ロジックを実装しています。<br>
・慣性射出と加速：発射直後の <code>delayGuideTime</code>（0.4 秒）は母機の速度を継承して直進し、その後に加速しながら誘導を開始。<br>
・ドッグファイトの数学：旋回性能（<code>turnSpeed</code>）に角速度の上限を設け、ターゲットが急旋回や高速交差を行うとミサイルが追い切れず「振り切れる」状況を再現しています。
</p>

<p>
The missile behavior reflects my personal design preferences. It features an initial straight‑flight delay based on inherited momentum, followed by acceleration and active guidance.  
By limiting angular velocity (<code>turnSpeed</code>), the system mathematically allows out‑maneuvering scenarios where high‑G turns exceed the missile’s turning radius.
</p>

<h2>アーキテクチャ / Core Architecture</h2>

<p><b>MapManager.cs</b>: 7×7 グリッドの LOD マップ管理、メッシュ生成</p>
<p><b>Missile.cs / MissileLauncher.cs / LockOnSystem.cs</b>: 誘導・加速ロジック、ターゲット追尾、残弾管理、発射後のモデル非表示などミサイル関連</p>
<p><b>CameraController.cs</b>: カメラ追従</p>
<p><b>TacticalHUD.cs</b>: GUI ベースの HUD 描画</p>
<p><b>EnemyUFO.cs</b>: 各 UFO インスタンスの挙動</p>
<p><b>PlayerController.cs</b>: プレイヤーの座標・回転・アニメーション制御</p>
<p><b>GameManager.cs</b>: 敵生成、プレイ時間管理、ポーズ、ゲームオーバー／クリア、リスタートまでを統括するゲーム進行管理</p>

<h2>開発における AI の活用 / AI Collaboration</h2>

<p>
本プロジェクトでは Gemini（Gemini 3 Flash）を「設計検証」および「ペアプログラマー」として活用しました。  
単なるコード生成ではなく、クラス責務の分配、エラーログ解析、設計思想の議論など、技術的対話を通じて開発効率と品質を両立しています。
</p>

<p>
I collaborated with Gemini 3 Flash as a technical partner for architecture validation and pair programming.  
The dialogue focused on responsibility allocation, error log analysis, and design discussions to ensure both development efficiency and software quality.
</p>

<h3>3. 自己分析ツールの開発 / Self‑Reflective Tooling</h3>
<p>
プロジェクトが複雑化する中で設計の脆さを客観的に把握するため、Unity 6 の実験的 API（Graph Node）を用いてクラス依存関係を可視化する <b>CodesVisualizer</b> を自作しました。
</p>

<p>
To understand architectural weaknesses as the project grew complex, I developed <b>CodesVisualizer</b> using Unity 6’s experimental Graph Node API to visualize class dependencies.
</p>

<h3>BlueJ への回帰 / Returning to Roots</h3>
<p>
コミュニティカレッジ時代に触れた Java 教育用 IDE「BlueJ」の視認性を Unity 上で再現しようと試行。  
ドキュメントの少ない実験的 API を扱うにあたり、AI（Gemini）を技術パートナーとして 7 時間の対話とデバッグを経て完成させました。
</p>

<p>
Inspired by “BlueJ” from my Community College days in the US, I aimed to recreate its visual clarity within Unity.  
With AI assistance navigating undocumented experimental APIs, the tool was completed after seven hours of intensive dialogue and debugging.
</p>

<h3>God Objectの露呈 / Exposing the God Object</h3>
<p>
可視化により、GameManager への過度な依存（God Object 化）やスパゲッティコード化した現状が、赤と青のラインとして露骨に浮かび上がりました。
</p>

<p>
The visualization revealed an over‑reliance on the GameManager as a God Object, along with the tangled, spaghetti‑like dependencies across the project.
</p>

<h2>教訓 / Lesson</h2>

<p>
「理想を具体的な成果物へ落とし込む」過程で、「とりあえず動けばいい」という突貫実装が招く設計負債を改めて痛感しました。  
かつて Java で制作したゲームと同様、今回も GameManager に責務が集中し、いわゆる God Object が再び生まれてしまったことを可視化ツールによって認識できました。  
この経験を通じて、疎結合で堅牢なオブジェクト指向設計を学び直す必要性を強く感じています。
</p>

<p>
In the process of turning ideas into a concrete product, I once again realized how “just making it work” leads to significant architectural debt.  
Just like the Java game I built years ago, this project revealed a recurrence of the God Object pattern, with too many responsibilities accumulating inside the GameManager — something made clear through the visualization tool.  
This experience reinforced the importance of relearning and prioritizing decoupled, robust Object‑Oriented Design.
</p>
