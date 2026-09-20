# Referensi Meeting RS MMC — Dampaknya pada Keputusan Finance

| Field | Nilai |
|---|---|
| Sumber | `Referensi_Meeting_FIN-OQ_dan_Medical_Fee.pdf`, disampaikan owner 20 September 2026 |
| Isi sumber | Rangkuman transcript meeting RS MMC untuk `FIN-OQ-006`, `FIN-OQ-008`, `FIN-OQ-009`, dan pembahasan Medical Fee |
| Transcript yang dirujuk | "Akutansi Zoom Meeting" 15 Juli 2026 · "Keuangan AP Zoom Meeting" 10 Juni 2026 dan 24 Juni 2026 · "Keuangan Zoom Meeting" 5 Agustus dan 29 Juli 2026 · "Meeting Lanjutan dengan Bag.Keuangan" 24 April 2026 |
| Sifat | **Bukti lapangan atas praktik yang sedang berjalan**, bukan keputusan baru |
| Dampak | Menguatkan tiga keputusan yang sudah `approved`; menambah satu peringatan; memperluas satu pertanyaan terbuka. **Nol keputusan yang dicabut atau diubah** |

Berkas ini mencatat apa yang berubah pada blueprint Finance karena sumber di atas. Isi lengkap
sumbernya ada pada dokumen aslinya, bukan disalin ulang di sini.

---

## 1. Tiga keputusan yang dikuatkan

| Keputusan | Yang dikuatkan | Bukti |
|---|---|---|
| `FIN-DEC-019` — pembayaran dokter direkap periodik | Praktik RS MMC memang rekap bulanan per dokter, ditarik dari billing yang sudah ditutup untuk periode tanggal 1 sampai akhir bulan | "Akutansi Zoom Meeting" 15 Juli 2026 |
| `FIN-DEC-021` — pembalikan alokasi membuka kembali piutang | Pola "Batal Settlement" existing memang reversal-bukan-delete, dan tagihan otomatis dapat di-settle ulang | "Keuangan AP Zoom Meeting" 10 Juni 2026 |
| `FIN-DEC-022` — approval AP berjenjang, berbeda dari AR | Sistem lama **tidak punya pemisahan pengaju/penyetuju sama sekali** untuk fee dokter; tiga orang akuntansi sama-sama membuat dan menyetujui | "Akutansi Zoom Meeting" 15 Juli 2026 |

Alasan yang owner sebut saat memutuskan `FIN-DEC-019` — dokter dobel atau lupa dibayar karena
rekonsiliasi lewat Excel terpisah — ternyata terdokumentasi sebagai keluhan tim RS MMC sendiri,
bukan kekhawatiran yang diperkirakan.

## 2. Satu peringatan baru

**`FIN-DEC-021` hanya sebagian berpijak pada pola yang sudah terbukti.**

Transcript membahas pembalikan hanya untuk skenario **penuh** — membatalkan seluruh settlement
atau membatalkan invoice. Skenario **sebagian**, yaitu pasien membayar sebagian lalu sisanya
menjadi piutang dan kemudian pembayarannya dibalik, **belum pernah ada** di sistem lama. Tim RS
MMC juga mengakui fitur Batal Settlement itu sendiri belum matang.

Konsekuensi yang sudah dicatat pada decision log: penanganan pembalikan untuk kasus sebagian
MUST diuji khusus, dan **tidak boleh** dianggap aman dengan alasan "sistem lama sudah begitu".
Uji `UAT-12` pada `04-prd-to-mvp.md` menguji tepat skenario ini.

## 3. Satu pertanyaan terbuka yang diperluas

`FIN-OQ-010` semula hanya menanyakan satu angka ambang. Setelah bukti ini masuk, ia diperluas
menjadi:

1. Ambang ditetapkan **per jenis transaksi** — supplier, dokter, dan write-off AR terpisah,
   karena karakter risikonya berbeda. Honor dokter adalah rekap bulanan berisi puluhan sampai
   ratusan fee per dokter; write-off AR umumnya keputusan per-transaksi tunggal.
2. Jejak audit MUST mencatat **tingkat approval mana** yang berlaku, bukan hanya siapa yang
   menyetujui.

## 4. Angka skala yang berguna untuk pengujian

| Angka | Nilai | Dipakai untuk |
|---|---|---|
| Jumlah dokter per bulan | ±220 | Ukuran rekap pembayaran; menguji bahwa satu pembayaran dengan ratusan baris alokasi tetap wajar performanya |
| Lama proses sekarang | ±4 hari kerja | Pembanding; proses baru MUST lebih singkat karena approve tidak lagi satu-per-satu |
| Tahap pembayaran | 2 kali sebulan, tanggal 5 dan 10 | Pola periode pembayaran |

## 5. Yang diteruskan ke modul lain

| Temuan | Diteruskan ke | Alasan |
|---|---|---|
| Tipe dokter (Full Time, Part Time, Dokter Tamu) punya skema sharing berbeda; tarifnya mengikuti PKS/SK masing-masing | **Medical Fee** | Aturan perhitungan fee bukan scope Finance |
| Pembagian tim untuk Lab/Radiologi/Anestesi/Digestive/Urologi masih manual lewat Excel satelit | **Medical Fee** | Menguatkan kebutuhan banyak dokter per layanan |
| Penyesuaian sebelum bayar — PPh 21, kasbon dokter, potongan hutang pasien yang dijamin potong honor, sitting fee, KSO, iuran kerohanian | **Medical Fee dan/atau Finance** | Batas antara keduanya belum diputuskan; diangkat pada wawancara Medical Fee |
| Pembayaran dua tahap dipisah berdasarkan status bayar pasien/asuransi | **Finance** | Menyiratkan Finance perlu tahu status bayar tagihan sumber per fee — titik sentuh yang belum dirancang, dicatat sebagai `FIN-OQ-012` |
| Diskon dokter memerlukan persetujuan dokter yang bersangkutan | **Billing** | Sudah ada `BilDiscountApplication` dengan approval; di luar scope Finance |

## 6. Pertanyaan terbuka baru untuk Finance

| ID | Pertanyaan | Memblokir |
|---|---|---|
| `FIN-OQ-012` | Pembayaran honor dokter di RS MMC dipisah dua tahap berdasarkan apakah pasien atau asuransi sudah membayar ke rumah sakit. Apakah pola itu dipertahankan di V2? Bila ya, Finance perlu cara mengetahui status bayar tagihan sumber untuk setiap utang dokter — dan jalur datanya belum dirancang. | `DESIGN` untuk `EPIC FIN-09` (Pembayaran) — sudah `POST-MVP`, tidak menahan MVP |
