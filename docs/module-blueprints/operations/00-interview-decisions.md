# Modul Operasi — Keputusan Wawancara

| Field | Nilai |
|---|---|
| Blueprint ID | `operations` |
| Revision | `6` |
| Status | `approved` |
| Product/domain owner | Pemilik kebutuhan |
| Backend SHA | `767470f742bc6f2eebadbd653a873f69d6f93121` |
| Frontend SHA | `400104f2a0f3239c14c40f5905b419977a538450` |

## Scope dan Outcome

Modul Operasi menangani proses kamar operasi atau perioperatif untuk operasi elektif dan darurat. Alur dimulai ketika permintaan operasi dibuat oleh dokter dan berakhir ketika pasien keluar dari ruang pemulihan serta diserahterimakan ke unit tujuan.

### Termasuk dalam scope

1. Daftar pasien operasi.
2. Penjadwalan ruang dan tim operasi.
3. Verifikasi pasien, persiapan praoperasi, dan checklist keselamatan.
4. Pencatatan pelaksanaan operasi, anestesi, bahan, implant, dan catatan operasi.
5. Pemantauan ruang pemulihan dan serah terima pasien.
6. Laporan operasi.

### Di luar kepemilikan Modul Operasi

- Diagnosis dan keputusan awal tindakan tetap milik layanan asal, seperti Rawat Jalan, IGD, atau Rawat Inap.
- Stok obat, bahan medis, dan implant tetap dikelola modul Farmasi atau Persediaan.
- Tagihan tetap diproses oleh Billing.
- Perawatan pasien setelah serah terima tetap menjadi tanggung jawab unit tujuan.

## Aktor dan Tanggung Jawab

| Aktor | Tanggung jawab utama |
|---|---|
| Dokter penanggung jawab/dokter bedah | Membuat permintaan operasi dan memastikan indikasi klinis |
| Koordinator kamar operasi | Menetapkan jadwal, ruang, dan tim serta menangani perubahan operasional |
| Dokter bedah utama | Memimpin operasi dan mengesahkan catatan operasi |
| Dokter anestesi | Menilai kelayakan anestesi dan memutuskan pasien boleh keluar dari ruang pemulihan |
| Perawat kamar operasi | Memastikan persiapan dan checklist, membantu pencatatan, serta melakukan serah terima |

## Business Rules dan Invariants

1. Permintaan operasi hanya dapat dibuat oleh dokter penanggung jawab atau dokter bedah.
2. Koordinator kamar operasi menetapkan jadwal, ruang, dan tim. Sistem harus mencegah benturan ruang, dokter, atau anggota tim pada waktu yang sama.
3. Kesiapan pasien harus dikonfirmasi bersama oleh dokter bedah, dokter anestesi, dan perawat kamar operasi.
4. Operasi darurat boleh memakai jalur darurat terkendali apabila penundaan membahayakan pasien. Bagian yang dilewati, alasan, waktu, dan penanggung jawab wajib dicatat dan dilengkapi setelah pasien stabil.
5. Operasi elektif yang terdampak operasi darurat dijadwalkan ulang oleh koordinator dengan konfirmasi dokter terkait.
6. Pembatalan klinis hanya dapat diputuskan oleh dokter bedah atau dokter anestesi dan wajib disertai alasan.
7. Obat, bahan medis, dan implant dicatat saat digunakan kepada pasien. Untuk implant, nomor batch atau serial juga dicatat. Modul Operasi mengirim catatan pemakaian kepada modul pemilik stok dan Billing.
8. Catatan operasi final disahkan oleh dokter bedah utama.
9. Catatan final tidak boleh ditimpa. Kesalahan diperbaiki melalui addendum yang menyimpan alasan, perubahan, waktu, dan identitas dokter.
10. Pasien hanya boleh keluar dari ruang pemulihan atas keputusan dokter anestesi. Perawat mencatat pemantauan dan serah terima.
11. Lifecycle utama kasus operasi adalah `Requested` → `Scheduled` → `Ready` → `In Progress` → `Completed`. Status `Postponed` digunakan untuk penundaan atau penjadwalan ulang, sedangkan `Cancelled` digunakan untuk pembatalan.
12. Dokter wajib mengisi encounter/pasien, tindakan, diagnosis atau indikasi, jenis elektif/darurat, prioritas, lokasi/sisi tindakan bila relevan, dokter bedah utama, perkiraan durasi, waktu yang diharapkan, kebutuhan khusus, dan catatan klinis penting ketika meminta operasi.
13. Kewenangan transition dibagi: dokter membuat `Requested`; koordinator menetapkan `Scheduled` dan `Postponed`; sistem menetapkan `Ready` setelah sign-off dokter bedah, dokter anestesi, dan perawat lengkap; dokter bedah menetapkan `In Progress`; sistem menetapkan `Completed` setelah catatan operasi selesai, dokter anestesi mengizinkan keluar recovery, dan serah terima diterima; dokter bedah atau dokter anestesi menetapkan `Cancelled` sebelum operasi dimulai.
14. Operasi darurat mendapat prioritas di atas operasi elektif. Estimasi durasi berasal dari permintaan dan dapat dikoreksi koordinator. Buffer persiapan/pembersihan dikonfigurasi, bukan angka tetap. Semua perubahan jadwal menyimpan jadwal lama, jadwal baru, alasan, dan pelaku.
15. Tim minimum terdiri dari dokter bedah utama, dokter anestesi, perawat instrumen, dan perawat sirkuler. Asisten bedah atau tenaga lain diwajibkan sesuai jenis tindakan. Hanya tenaga aktif dengan kewenangan klinis yang sesuai yang boleh ditugaskan.
16. Checklist keselamatan dibagi menjadi verifikasi sebelum anestesi, verifikasi sebelum insisi, dan verifikasi sebelum pasien keluar dari ruang operasi. Identitas pasien, tindakan/lokasi, consent, alergi, kesiapan anestesi, kesiapan alat/implant, dan hitung instrumen termasuk pemeriksaan wajib yang relevan. Jalur darurat harus mencatat item yang dilewati dan melengkapinya setelah pasien stabil.
17. Catatan operasi minimal mencakup diagnosis pra/pascaoperasi, tindakan, tim, waktu, temuan, teknik, komplikasi, perdarahan, spesimen, drain/implant, serta rencana pascaoperasi. Catatan anestesi terpisah memuat asesmen, teknik anestesi, obat/cairan, pemantauan, airway, kejadian, dan kondisi akhir.
18. Obat, bahan, alat, dan implant disiapkan sebelum operasi tetapi stok aktual dikurangi sesuai pemakaian. Barang utuh yang belum dipakai dapat diretur setelah verifikasi; barang terbuka/tidak layak dicatat sebagai waste. Implant wajib mencatat batch/serial. Koreksi memerlukan alasan dan audit.
19. Recovery memakai kriteria/skor klinis yang dikonfigurasi rumah sakit. Dokter anestesi memutuskan pasien keluar; pasien yang belum memenuhi kriteria tetap dipantau atau dipindahkan ke unit yang sesuai. Serah terima mencatat kondisi, alat terpasang, terapi, risiko, instruksi, pemberi, penerima, tujuan, dan waktu.
20. `Cancelled` hanya berlaku sebelum `In Progress`. Jika tindakan dihentikan setelah dimulai, lifecycle tetap diselesaikan sebagai `Completed` dengan hasil `StoppedEarly`, alasan, kondisi pasien, dan pemakaian aktual. Dampak stok dan tagihan direkonsiliasi berdasarkan kejadian nyata.
21. Charge tindakan dibuat ketika tindakan selesai. Obat, bahan, dan implant dibebankan berdasarkan pemakaian aktual. Setiap handoff ke Billing harus idempotent agar pengiriman ulang tidak membuat tagihan ganda. Pembatalan/koreksi menghasilkan reversal atau koreksi melalui Billing, bukan menghapus histori.
22. Laporan minimum meliputi daftar/jadwal operasi, pembatalan/penundaan, pemakaian ruang, durasi, tindakan, tim, komplikasi, implant/material, recovery, serta audit perubahan. Notifikasi dikirim kepada tim/unit terdampak ketika jadwal dibuat, diubah, ditunda, dibatalkan, atau pasien siap diserahterimakan; kanal notifikasi dapat dikonfigurasi.

**Contoh benturan jadwal:** Ruang Operasi 1 sudah digunakan pukul 09.00–11.00. Sistem menolak penjadwalan pasien lain di ruang yang sama pada pukul 10.00.

**Contoh jalur darurat:** Pasien dari IGD membutuhkan operasi segera untuk menyelamatkan nyawa. Dokter mencatat alasan darurat dan checklist yang belum dapat diselesaikan. Setelah pasien stabil, bagian tersebut dilengkapi tanpa menghapus jejak kejadian awal.

**Contoh addendum:** Catatan final menyebut implant ukuran 10 mm, tetapi bukti pemakaian menunjukkan 12 mm. Dokter tidak mengganti catatan asli, melainkan membuat addendum yang menjelaskan koreksi menjadi 12 mm beserta alasannya.

### Lifecycle Kasus Operasi

| Dari status | Tindakan | Ke status | Keterangan |
|---|---|---|---|
| - | Dokter mengirim permintaan operasi | `Requested` | Permintaan sudah tercatat dan menunggu penjadwalan |
| `Requested` | Menetapkan ruang, waktu, dan tim | `Scheduled` | Jadwal operasi sudah ditetapkan |
| `Scheduled` | Seluruh pemeriksaan kesiapan disetujui | `Ready` | Pasien dan tim dinyatakan siap |
| `Ready` | Memulai tindakan | `In Progress` | Operasi sedang berjalan |
| `In Progress` | Menyelesaikan seluruh kasus perioperatif | `Completed` | Catatan operasi selesai, recovery disetujui, dan serah terima diterima |
| `Requested` atau `Scheduled` | Menunda atau menjadwalkan ulang | `Postponed` | Koordinator menyimpan alasan dan riwayat jadwal |
| `Postponed` | Menetapkan jadwal baru | `Scheduled` | Ruang dan tim tersedia tanpa benturan |
| `Requested`, `Scheduled`, atau `Ready` | Membatalkan | `Cancelled` | Dokter bedah/dokter anestesi menyimpan alasan klinis |

Setelah status `In Progress`, kasus tidak boleh menjadi `Cancelled`. Tindakan yang harus dihentikan dicatat sebagai `Completed` dengan hasil `StoppedEarly` agar pelaksanaan, komplikasi, material, dan tagihan aktual tetap terlacak.

## Alur Bisnis Garis Besar

1. Dokter membuat permintaan operasi dari layanan pasien.
2. Koordinator menetapkan jadwal, ruang, dan tim tanpa benturan sumber daya.
3. Dokter bedah, dokter anestesi, dan perawat memeriksa kesiapan pasien.
4. Tim menjalankan operasi serta mencatat anestesi, tindakan, bahan, obat, dan implant yang benar-benar digunakan.
5. Dokter bedah utama mengesahkan catatan operasi.
6. Pasien dipantau di ruang pemulihan.
7. Dokter anestesi menyatakan pasien boleh keluar dari ruang pemulihan.
8. Perawat menyerahkan pasien kepada unit tujuan dan mencatat serah terima.

## Decision Log

| Decision ID | Type | Keputusan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `OPS-DEC-001` | Decision | Scope dibatasi pada proses kamar operasi/perioperatif | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-21 | Wawancara `grill-me` |
| `OPS-DEC-002` | Decision | Mencakup operasi elektif dan darurat | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-21 | Wawancara `grill-me` |
| `OPS-DEC-003` | Decision | Permintaan operasi dibuat dokter penanggung jawab/dokter bedah | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-21 | Wawancara `grill-me` |
| `OPS-DEC-004` | Decision | Koordinator menetapkan jadwal, ruang, dan tim | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-21 | Wawancara `grill-me` |
| `OPS-DEC-005` | Decision | Kesiapan disetujui dokter bedah, dokter anestesi, dan perawat | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-21 | Wawancara `grill-me` |
| `OPS-DEC-006` | Decision | Operasi darurat memakai jalur darurat terkendali | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-21 | Wawancara `grill-me` |
| `OPS-DEC-007` | Decision | Jadwal elektif dapat digeser koordinator dengan konfirmasi dokter | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-21 | Wawancara `grill-me` |
| `OPS-DEC-008` | Decision | Pembatalan klinis hanya oleh dokter bedah atau dokter anestesi | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-21 | Wawancara `grill-me` |
| `OPS-DEC-009` | Decision | Pemakaian bahan dan implant dicatat saat digunakan | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-21 | Wawancara `grill-me` |
| `OPS-DEC-010` | Decision | Dokter bedah utama mengesahkan catatan operasi | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-21 | Wawancara `grill-me` |
| `OPS-DEC-011` | Decision | Koreksi catatan final menggunakan addendum | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-21 | Wawancara `grill-me` |
| `OPS-DEC-012` | Decision | Dokter anestesi memutuskan pasien keluar dari ruang pemulihan | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-21 | Wawancara `grill-me` |
| `OPS-DEC-013` | Decision | Lifecycle kasus operasi memakai `Requested`, `Scheduled`, `Ready`, `In Progress`, `Completed`, `Postponed`, dan `Cancelled` | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-21 | Wawancara lanjutan `grill-me` |
| `OPS-DEC-014` | Decision | Data minimum permintaan operasi mengikuti rekomendasi `OPS-REQ-001` | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-21 | Persetujuan seluruh rekomendasi |
| `OPS-DEC-015` | Decision | Kewenangan transition dibagi sesuai tanggung jawab klinis dan operasional | Pemilik kebutuhan | superseded | Digantikan `OPS-DEC-025`, 2026-08-21 | Persetujuan seluruh rekomendasi |
| `OPS-DEC-016` | Decision | Prioritas, estimasi, buffer terkonfigurasi, dan histori reschedule mengikuti rekomendasi | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-21 | Persetujuan seluruh rekomendasi |
| `OPS-DEC-017` | Decision | Tim minimum dan validasi kewenangan klinis mengikuti rekomendasi | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-21 | Persetujuan seluruh rekomendasi |
| `OPS-DEC-018` | Decision | Checklist tiga tahap dan bypass darurat terkendali digunakan | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-21 | Persetujuan seluruh rekomendasi |
| `OPS-DEC-019` | Decision | Data minimum catatan operasi dan anestesi mengikuti rekomendasi | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-21 | Persetujuan seluruh rekomendasi |
| `OPS-DEC-020` | Decision | Pemakaian, retur, waste, implant, dan koreksi stok mengikuti rekomendasi | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-21 | Persetujuan seluruh rekomendasi |
| `OPS-DEC-021` | Decision | Recovery dan serah terima mengikuti rekomendasi | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-21 | Persetujuan seluruh rekomendasi |
| `OPS-DEC-022` | Decision | Pembatalan hanya sebelum mulai; penghentian setelah mulai memakai hasil `StoppedEarly` | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-21 | Persetujuan seluruh rekomendasi |
| `OPS-DEC-023` | Decision | Charge tindakan saat selesai dan material sesuai pemakaian aktual dengan idempotency | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-21 | Persetujuan seluruh rekomendasi |
| `OPS-DEC-024` | Decision | Laporan dan notifikasi minimum mengikuti rekomendasi | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-21 | Persetujuan seluruh rekomendasi |
| `OPS-DEC-025` | Decision | `Completed` berarti seluruh kasus perioperatif selesai dan ditetapkan sistem setelah catatan operasi, recovery, serta serah terima lengkap | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-21 | Persetujuan koreksi arsitektur domain |
| `OPS-DEC-026` | Decision | Tiga sign-off kesiapan disimpan sebagai baris `OprStatusHistory` beridentitas `Action = "ReadinessSignOff"`, bukan tabel tersendiri | Pemilik kebutuhan | approved | Pemilik kebutuhan, 2026-08-24 | Keputusan saat implementasi `BE-OPR-005` |

### OPS-DEC-026 — Tempat penyimpanan sign-off kesiapan

| Field | Nilai |
|---|---|
| Status | `approved` |
| Owner | Pemilik kebutuhan |
| Keputusan | Setiap sign-off kesiapan dicatat sebagai satu baris `OprStatusHistory` dengan `Action = "ReadinessSignOff"`. Peran penanda tangan disimpan pada kolom `Reason`, dan catatan tambahan dipisahkan dengan tanda `\|` |
| `approved_by` / `approved_at` | Pemilik kebutuhan / 2026-08-24 |

**Masalah yang diputuskan.** `opr-api-v1` mensyaratkan endpoint `POST /sign-offs`, tetapi ERD
yang disetujui tidak memuat tabel sign-off. Satu-satunya tempat yang tersedia adalah
`OprSafetyChecklist`, dan tabel itu hanya menyediakan **satu** penanda tangan per fase dengan
kunci unik `case + phase + revision`. Tiga sign-off dari tiga peran berbeda tidak muat di sana.

**Alasan memilih `OprStatusHistory`.** Tabel ini sudah bersifat append-only dan sudah membawa
`ActorUserId`, `OccurredAt`, `CorrelationId`, serta `Source`, sehingga seluruh kebutuhan audit
sign-off sudah terpenuhi. Tidak perlu entity baru, tidak perlu migration tambahan, dan tidak
perlu menaikkan revision blueprint. Preseden pemakaian `Action` untuk kejadian yang bukan
perpindahan status sudah ada di modul ini, yaitu `Action = "UpdateRequest"`.

**Contoh isi baris.**

> Dokter anestesi bernama samaran "Dokter B" memberi sign-off pukul 07.40 dengan catatan
> "Pasien puasa sejak 22.00". Sistem menyimpan satu baris `OprStatusHistory` dengan
> `Action = "ReadinessSignOff"`, `Reason = "Anesthesiologist|Pasien puasa sejak 22.00"`,
> `FromStatus = Scheduled`, `ToStatus = Scheduled`, dan `ActorUserId` milik Dokter B.
> Ketika sign-off ketiga masuk, sistem menulis **satu** baris tambahan dengan
> `Action = "CompleteReadiness"`, `FromStatus = Scheduled`, dan `ToStatus = Ready`.

**Konsekuensi yang diterima.** Peran penanda tangan tidak menjadi kolom tersendiri, sehingga
pemeriksaan "apakah dokter bedah sudah tanda tangan" berupa pencocokan teks, bukan pencarian
lewat index. Bila kelak sign-off perlu disaring atau dilaporkan dalam jumlah besar, keputusan
ini perlu ditinjau ulang dan diganti tabel `OprReadinessSignOff` tersendiri.

**Alternatif yang ditolak.** Menyimpan sign-off di dalam `ItemsJson` milik checklist ditolak
karena status bisnis yang menentukan perpindahan ke `Ready` akan hidup di dalam JSON bebas
tanpa skema, sulit diaudit per orang, dan bertentangan dengan semangat aturan project soal
rule production di dalam JSON.

## Open Questions dan Blocker

- Kemampuan yang tersedia sudah diaudit dalam `01-existing-capability-map.md`.
- Kontrak API, struktur data, dan integrasi target belum dirancang; hal tersebut merupakan pekerjaan tahap arsitektur, bukan keputusan wawancara.
- Baseline canonical `Operating Theatre` pada `indonesia-hospital-domain-reference` belum tersedia. Keputusan di atas adalah persetujuan pemilik kebutuhan terhadap rekomendasi, bukan klaim regulasi atau SOP rumah sakit.
