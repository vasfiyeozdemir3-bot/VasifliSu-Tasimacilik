# Vasıflı Su Taşımacılık – Dağ Yolu

**Öğrenci:** Vasfiye Özdemir (2506127016) • **Ders:** Oyunlaştırma

▶ **Tarayıcıda oyna (WebGL):** https://vasfiyeozdemir3-bot.github.io/VasifliSu-Tasimacilik/

Bir su dağıtım firmasının markalaşma ve lojistik sürecini oyunlaştıran, **WheelJoint2D** fizik motoruyla çalışan, Hill Climb Racing tarzı 2B fizik oyunu.
Kasasında 19 L damacanalar, ortasında "İÇME SUYU" tankı olan Vasıflı Su kamyonunu engebeli arazide devirmeden sür ve suyu müşteriye ulaştır.

## Bölümler
1. **Volkan Eteği** – lav akıntılı volkan dağı, öğretici
2. **Şehir Merkezi** – gece şehri, bariyer ve koni engelleri
3. **Gün Batımı Vadisi** – dik yokuşlar
4. **Kartopu Kasabası** – karlı evler, kaygan zemin, kar yağışı
5. **Mavi Liman** – liman / sanayi bölgesi, final

Bölümler sırayla açılır: her depo kapısı, önceki teslimat başarıyla tamamlanınca açılır.

## Kontroller
| Eylem | Klavye | Gamepad |
|---|---|---|
| Gaz | → / ↑ / SHIFT / D / W | Sağ tetik |
| Fren / Geri | ← / ↓ / CTRL / BOŞLUK / A / S | Sol tetik |
| Havada | Gaz = burun yukarı, Fren = burun aşağı | |
| Duraklat | ESC / P | Start |
| Yeniden başla | R | – |

## Menü akışı
Ana Menü (Oyna / Ayarlar / GDD / Çıkış) → Depo Kapıları (bölüm seçimi) → Oynanış (hız, su, skor) → Duraklatma (devam / yeniden başlat / ana menü) → Sonuç (teslim oranı, yıldız, skor)

## Teknik
- Unity 6.3 LTS (6000.3.28f1), 2D, Built-in RP, yeni Input System
- Fizik: `TruckController` – iki `WheelJoint2D` (JointMotor2D + JointSuspension2D), hava kontrolü, 34 km/s hız sınırı
- Kargo: her damacana ayrı `Rigidbody2D`; yere düşen damacana kaybolur
- Coin sistemi: her bölümde yol boyunca toplanabilir coinler (+50 skor)
- Prosedürel arazi (`TerrainGenerator`), ScriptableObject bölüm verileri (`LevelData`)
- Türkçe / English dil desteği, ses ayarları, PlayerPrefs kayıt sistemi
- Editör menüsü **Vasıflı Su**: 1) Projeyi Kur, 2) Windows Build, 3) WebGL Build, 4) Tanıtım Videosu Kaydet, 5) 5 Bölümü Otomatik Test Et

## Klasör düzeni
```
Assets/_VasifliSu/
  Art/ (UI, Icons, Vehicle, Environment, Ground, Backgrounds)
  Audio/   Fonts/   Data/   Materials/   PhysicsMaterials/   Prefabs/   Resources/
  Scenes/  (MainMenu, LevelSelect, Level_1..Level_5)
  Scripts/ (Core, Vehicle, Level, UI, Editor)
Assets/StreamingAssets/VasifliSu_GDD.pdf
```

## Teslim dosyaları
- GDD: `Assets/StreamingAssets/VasifliSu_GDD.pdf`
- WebGL build: `gh-pages` dalı (GitHub Pages)
- Windows build ve tanıtım videosu: USB teslimi
