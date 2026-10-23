# Laporan Kesiapan — Amendment RP: Reservasi Pasien Prioritas (Rawat Jalan)

| Field | Nilai |
| --- | --- |
| Modul / amendment | `rawat-jalan`, *Amendment RP* (Reservasi Pasien Prioritas) |
| Revisi blueprint | `33`, `approved` (Sukma Giri, 16 Oktober 2026, `RJ-PRI-DEC-030`). Bentuk `SINGLE` |
| Kontrak | `RJ-PRI-RESV-001@1.0.0` `approved` |
| Roadmap | `RJ-PRI-RM-BE-001` r1 dan `RJ-PRI-RM-FE-001` r1, `approved` (`RJ-PRI-DEC-031`) |
| Keputusan terkait | `RJ-PRI-DEC-001`..`031`; terbuka `RJ-PRI-OQ-002`, `003`, `004`, `005`, `006`, `011`, `012` |
| Snapshot backend | `NewQuilvianSystemBackend` `sukmagp` @ `a6499cdd` + working tree belum di-commit |
| Snapshot frontend | `V2QuilvianSystemFrontendDev` `sukmagpV2` @ `17e0ffec0` + working tree belum di-commit |
| Database uji | `QuilvianNewDevSukma` (migration `20261016144037_AddOutpatientPriorityReservation` sudah diterapkan) |
| Tanggal audit | 19 Oktober 2026 |
| Auditor | Claude Opus 5 (`verify-module-readiness`), read-only terhadap source |
| **Verdict** | **`READY_WITH_CONDITIONS`** — kode dan bukti uji lengkap; peluncuran ke pasien menunggu 7 kondisi di bagian 4 |

## 1. Ringkasan untuk pengambil keputusan

Fitur ini sudah selesai dibangun dan teruji di lingkungan pengembangan. Seluruh 20 task (12 backend, 8 frontend) punya
laporan dan bukti uji. Seluruh 30 skenario uji teknis (`R-RP-01`..`30`) dan 19 skenario UAT (`UAT-RP-01`..`19`)
sudah dijalankan dengan data samaran.

**Yang sudah berfungsi di lingkungan pengembangan:**

1. **Reservasi lewat WA/telepon.** Petugas admisi membuat reservasi untuk pasien member Aktif tanpa memilih poli atau
   dokter. Sistem menolak pasien Reguler dan reservasi ganda di tanggal yang sama, lalu menerbitkan nomor
   `RSV-20261019-0062`, link konfirmasi, dan teks pesan WA siap kirim.
2. **Pasien tiba.** Petugas menandai tiba, atau pasien menekan "Saya sudah tiba" di HP. Layar admisi dan petugas
   skrining langsung berbunyi dan menampilkan "Pasien prioritas tiba".
3. **Skrining awal dan pendaftaran.** Petugas skrining mengisi Batuk, Demam, dan Risiko jatuh. Admisi melengkapi
   pendaftaran lewat wizard Pendaftaran Pasien dengan pasien dan tanggal terkunci. Nomor antrean prioritas kelipatan 5
   terbit, misalnya `I100`.
4. **Antrean prioritas tersembunyi dari layar display.** Nomor prioritas tidak tampil dan tidak dipanggil suara di
   layar publik. Kartu Nurse Station dan Klinis Dokter menandainya "Prioritas" dan "Ruang Tunggu Prioritas".
5. **Kiosk.** Pasien Gold yang mendaftar sendiri di kiosk mendapat arahan ke ruang tunggu prioritas. Reservasinya yang
   masih terbuka berubah menjadi "Didaftarkan lewat Jalur Lain".
6. **Penutupan otomatis.** Reservasi yang pasiennya tidak datang ditutup pukul 00.05 WIB sebagai "Tidak Datang".

**Yang belum boleh dilakukan:** meluncurkan fitur ke pasien sebelum ketujuh kondisi di bagian 4 dipenuhi. Kondisi
terpenting adalah **K-1**: pesan WA selalu memuat link konfirmasi. Bila MVP-1 dirilis lebih dulu daripada halaman
link publik (MVP-2), pasien akan menerima link yang tidak bisa dibuka.

## 2. Kemajuan pembangunan vs kesiapan sesungguhnya

| Ukuran | Nilai | Catatan |
| --- | --- | --- |
| Task selesai (scaffold + implementasi) | 20/20 (100%) | Semua ✅ dan semua punya laporan tracked |
| Kesiapan ujung ke ujung (skor bagian 3) | **91,3%** | Kode lengkap; yang kurang adalah data master asli, konfigurasi produksi, dan keputusan urutan rilis |
| Go-live | **Belum** | Lihat bagian 4 |

Task yang selesai belum sama dengan siap dipakai. Contoh: kode antrean prioritas sudah benar, tetapi selama master
tingkat member belum diisi `PriorityQueue = true` (`RJ-PRI-OQ-006`), tidak ada pasien asli yang mendapat nomor
prioritas.

## 3. Skor kesiapan per dimensi

| Dimensi | Bobot | Skor | Bukti | Gap/blocker |
| --- | ---: | ---: | --- | --- |
| Fondasi dan data | 20% | 5/7 (71,4%) | `NewQuilvianSystemBackend/Areas/HealthServices/RegistrationManagement/Models/RegPriorityReservation.cs`@`a6499cdd`+WT; `Migrations/20261016144037_AddOutpatientPriorityReservation.cs`@WT; `has-pending-model-changes` bersih 19 Okt 2026; `appsettings.json#HealthServices:Registration:PriorityReservation`@WT | (6) master tingkat asli `PriorityQueue = true` belum ada (`RJ-PRI-OQ-006`); (7) `PublicArrivalBaseUrl` produksi belum disetel (bawaan `http://localhost:3000/priority-arrival`) |
| Backend | 25% | 12/12 (100%) | Laporan `task/report/backend/BE-RJP-001..012.md`; build 0 error dan QBE Strict 32 berkas `PASS` pada 19 Okt 2026 | Tidak ada. Catatan kecil di bagian 5 |
| Frontend | 25% | 8/8 (100%) | Laporan `task/report/frontend/FE-RJP-001..008.md`; `V2QuilvianSystemFrontendDev/src/components/view/health-services/registration-management/priority-reservations/`@`17e0ffec0`+WT; `src/app/priority-arrival/[code]/page.jsx`@WT; lint dan build exit 0 | Satu deviasi tercatat: label member belum di kartu hasil pencarian (`FE-RJP-002` bagian 6) |
| Integrasi dan runtime | 15% | 9/11 (81,8%) | `INT-RP-01`..`09` terbukti di dev (lihat tabel 3.1) | (10) domain publik link belum bisa diakses dari internet (`RJ-PRI-OQ-003`); (11) penerusan IP asli lewat reverse proxy belum terbukti |
| Verifikasi | 15% | 48/49 (98,0%) | 30/30 `R-RP` + 19/19 `UAT-RP` dirujuk laporan dengan hasil `PASS` | `UAT-RP-03` bagian suara pada perangkat display fisik belum diuji (`GAP-RP-06`) |
| **Total** | 100% | **91,3%** | — | **`READY_WITH_CONDITIONS`** |

Perhitungan total: 20 × 5/7 + 25 × 12/12 + 25 × 8/8 + 15 × 9/11 + 15 × 48/49 = 14,3 + 25 + 25 + 12,3 + 14,7 = 91,3.

### 3.1 Integrasi

| ID | Titik integrasi | Bukti | Status |
| --- | --- | --- | --- |
| `INT-RP-01` | Registration → resolver member aktif CRM | `BE-RJP-002` runtime 8/8 | Terbukti |
| `INT-RP-02` | Registration → status membership | `BE-RJP-002`, `BE-RJP-006` kelayakan | Terbukti |
| `INT-RP-03` | Reservasi → pendaftaran kunjungan | `BE-RJP-008` 16/16 + 2/2, `FE-RJP-006` 7/7 | Terbukti |
| `INT-RP-04` | Pendaftaran (kiosk) → reservasi Jalur Lain | `BE-RJP-008` `R-RP-21`, `FE-RJP-007` (`status=6`) | Terbukti |
| `INT-RP-05` | Reservasi → `NumberSeriesAllocator` | `BE-RJP-006` `R-RP-10` (paralel tanpa nomor ganda) | Terbukti |
| `INT-RP-06` | Registration → layar display (group sendiri) | `BE-RJP-004`, `FE-RJP-001` 12/12 | Terbukti; suara perangkat fisik menunggu UAT |
| `INT-RP-07` | Reservasi → board admisi (`PriorityReservationChanged`) | `BE-RJP-010` 9/9, `FE-RJP-005` `R-RP-14`, `FE-RJP-008` `UAT-RP-14` | Terbukti |
| `INT-RP-08` | Hosted service penutupan 00.05 WIB | `BE-RJP-009` 5/5 dua kali; hanya peran `Worker`/`All` | Terbukti (jadwal dihitung, tidak ditunggu sampai jam 00.05) |
| `INT-RP-09` | Frontend → WhatsApp (`wa.me`) | `FE-RJP-004` `UAT-RP-07` (`href` ke `wa.me/62…?text=`) | Terbukti untuk pembentukan tautan; pengiriman tetap manual oleh petugas |

### 3.2 Rantai requirement

Seluruh 28 baris pada [priority-reservation-traceability.md](../roadmap/priority-reservation-traceability.md)
(`FR-RP-001` s.d. `070`, regresi, dan butir menu) bertanda ✅. Setiap baris menunjuk keputusan, bagian desain, kontrak,
task backend/frontend, dan skenario uji. Seluruh 22 acceptance criteria di decision log punya skenario.

### 3.3 Kebaruan bukti (`STALE`)

| Masukan | Temuan | Dampak |
| --- | --- | --- |
| Kontrak dan desain | 15 dari 16 berkas sama persis dengan hash approval di manifest. `00-interview-decisions.md` berubah karena penambahan `RJ-PRI-DEC-031` (approval roadmap) | Tidak basi |
| Backend `551e1cbb` → `a6499cdd` | 4 commit milik user (merge 16 Okt 17.01 WIB). Di area task hanya `Program.cs` +2 baris (`RadStudyNumberService`, `RadPhotoNumberService`) | Tidak basi. Build dan QBE akhir dijalankan di `a6499cdd`. Uji layar 19 Okt memakai build dari kode yang sudah di-merge |
| Frontend `1fd858322` → `17e0ffec0` | 11 commit antara impact scan dan implementasi. Implementasi dan uji dikerjakan di `17e0ffec0` | Capability map frontend `STALE` sebagai catatan; bukti task tidak basi |
| Grafik dependency | Backend 10 pasangan dan frontend 11 pasangan, sama dengan kolom `Dependency`. Tanda node sama dengan kartu task | Tidak ada gap dokumentasi delivery |

## 4. Kondisi sebelum go-live (urut menurut dampak)

| ID | Kondisi | Dampak bila diabaikan | Pemilik | Mitigasi / jalan keluar |
| --- | --- | --- | --- | --- |
| **K-1** | **Urutan rilis vs isi pesan WA.** PRD menyatakan pada MVP-1 petugas yang menandai tiba, dan link publik baru di MVP-2. Kode saat ini selalu menerbitkan link dan menulis "buka link berikut lalu tekan Saya sudah tiba" di pesan WA (`OutpatientPriorityReservationService.BuildWhatsAppMessage`). Tidak ada pengaturan untuk mematikan link | Bila MVP-1 dirilis sendiri, pasien menerima link yang tidak bisa dibuka | Sukma Giri (Product) | Pilih salah satu: (a) rilis MVP-1 dan MVP-2 bersamaan sesudah `RJ-PRI-OQ-003` terjawab (**direkomendasikan** bila jawaban infrastruktur dekat; tanpa kode baru); (b) task kecil baru: pengaturan `PublicArrivalEnabled` yang menghapus link dan kalimat link dari pesan selama MVP-1 (perubahan kontrak minor, satu task BE + satu task FE); (c) petugas menghapus link secara manual sebelum mengirim (rawan lupa, tidak direkomendasikan) |
| **K-2** | `RJ-PRI-OQ-003`: domain publik link bisa dibuka dari HP pasien, reverse proxy meneruskan IP asli, dan `PublicArrivalBaseUrl` disetel ke alamat produksi (desain: `https://quilvian-mmchospital.com/priority-arrival`) | Link tidak terbuka, atau batas laju menghitung semua pasien sebagai satu IP proxy | IT/infrastruktur | Uji dari jaringan seluler sebelum go-live. Pastikan backend hanya bisa diakses lewat reverse proxy (lihat K-5) |
| **K-3** | `RJ-PRI-OQ-006`: master tingkat member asli diisi `PriorityQueue = true` | Tidak ada pasien yang mendapat nomor prioritas; reservasi tetap bisa dibuat untuk member Aktif | Yoga Aji Pratama / Marketing | Isi lewat layar Membership Tier; periksa satu pasien uji sebelum hari pertama |
| **K-4** | `GAP-RP-03`: centang `OutpatientPriorityReservation` (`Read`, `Create`, `Complete`, `Screen`, `Cancel`) di Akses Role sesuai `permission-audit-matrix.md` RP-2 | Butir menu tidak tampil; petugas skrining tidak bisa menyimpan skrining | Administrator sistem / Sukma Giri | Daftar periksa go-live; uji login satu akun admisi dan satu akun skrining |
| **K-5** | `GAP-RP-04`: tinjauan keamanan endpoint publik oleh manusia. Termasuk temuan audit ini: `ForwardedHeaders` mengosongkan `KnownNetworks`/`KnownProxies` (konfigurasi lama di `Program.cs`), sehingga header `X-Forwarded-For` dari klien dipercaya | Bila backend terbuka langsung tanpa proxy, batas laju per IP bisa diakali. Risiko rendah karena kode link 256-bit acak dan hanya hash-nya yang disimpan | Sukma Giri + peninjau keamanan | Tutup akses langsung ke backend, atau isi `KnownProxies` dengan IP reverse proxy (perubahan platform, di luar task ini) |
| **K-6** | Review diff, commit, dan penerapan migration `20261016144037_AddOutpatientPriorityReservation` ke lingkungan target | Fitur tidak ada di staging/produksi | Sukma Giri | Migration aditif (2 tabel baru); langkah mundur: `Down` menghapus kedua tabel |
| **K-7** | UAT bersama petugas sungguhan: suara pada perangkat display fisik (`GAP-RP-06`), alur skrining, dan pemberitahuan ke pemilik kiosk bahwa tiket prioritas kini memuat arahan ruang tunggu | Masalah di perangkat nyata baru ketahuan saat operasional | Sukma Giri + pemilik kiosk | Jadwalkan UAT memakai skenario `UAT-RP-01`..`19` |

## 5. Gap menurut kelompok

| Kelompok | Gap | Tingkat | Arah penyelesaian |
| --- | --- | --- | --- |
| Desain | K-1 urutan rilis vs pesan WA | Tinggi (go-live MVP-1) | `grill-me` Amendment Pass singkat untuk memilih (a)/(b)/(c); bila (b), `plan-module-delivery` menambah satu task BE + FE |
| Backend | Uji runtime `403` untuk `GET` daftar tanpa `Read` tidak dijalankan (atribut sudah diperiksa QBE; `403` untuk buat/tandai tiba/batal/lengkapi sudah diuji di `BE-RJP-007`/`008`) | Rendah | Bisa ikut UAT K-4 |
| Backend | Regresi batas laju "Cek No. RM" kiosk tidak diuji ulang (`BE-RJP-012`; cabang lama tidak berubah) | Rendah | Ikut UAT kiosk K-7 |
| Frontend | Label member di kartu hasil pencarian belum ada; menunggu suntingan user di `patient-selection-step.jsx` di-commit (`FE-RJP-002`) | Rendah | Satu task `build-module-frontend` kecil sesudah commit user |
| Frontend | Pesan kosong "Belum ada reservasi prioritas pada tanggal ini." juga tampil untuk pencarian tanpa hasil (`FE-RJP-004`) | Rendah (salinan teks) | Usulan revisi skema `FE-RP-01` |
| Integrasi | Domain publik dan IP asli (K-2) | Tinggi (go-live MVP-2) | IT/infrastruktur |
| Data/konfigurasi | Master tingkat (K-3); `PublicArrivalBaseUrl` produksi (K-2); Akses Role (K-4) | Tinggi | Pemilik masing-masing |
| Keamanan/privasi | K-5. Yang sudah terbukti: halaman publik tanpa nama, No. RM, tingkat, poli, dan dokter; kode tidak masuk log (`BE-RJP-012`); kode tidak tersimpan di browser (`FE-RJP-008`); `referrer: no-referrer` | Sedang | Tinjauan manusia |
| Test | `dotnet test` `NOT RUN` sesuai pola Bank Darah (verifikasi lewat build, EF, QBE, runtime HTTP, dan uji layar). Suara perangkat fisik (K-7) | Rendah | UAT |
| Operasional | Batas laju disimpan di memori per instance; bila backend dijalankan lebih dari satu instance, batasnya berlaku per instance | Rendah | Dicatat untuk tim infrastruktur |
| Di luar modul | Build backend memuat 11 warning kerentanan paket `SixLabors.ImageSharp` 3.1.11 (termasuk tingkat high) | Sedang, bukan milik amendment ini | Diteruskan ke pemilik platform backend |

## 6. Yang sudah terbukti untuk invariant kritis

| Invariant | Bukti |
| --- | --- |
| Status member tidak mengubah urutan medis; prioritas hanya nomor kelipatan 5 dan lokasi tunggu (`CRM-DEC-007`, `RJ-PRI-DEC-004`) | `BE-RJP-001`, `BE-RJP-008` |
| Nomor prioritas tidak tampil dan tidak bersuara di display publik (`RJ-PRI-DEC-005`) | `BE-RJP-003`, `BE-RJP-004`, `FE-RJP-001` |
| Satu reservasi terbuka per pasien per tanggal; tanpa nomor ganda saat paralel (`INV-RP-01`) | `BE-RJP-006` `R-RP-09`, `R-RP-10` |
| Perubahan oleh dua petugas tidak saling menimpa (`RowVersion`, 409 `VAL-RP-14`) | `BE-RJP-007`, `FE-RJP-005` STALE 409 |
| Link hanya disimpan sebagai hash; link lama mati saat diterbitkan ulang | `BE-RJP-006`, `BE-RJP-007`, `FE-RJP-008` `UAT-RP-15` |
| Kunjungan dan perubahan reservasi dalam satu transaksi | `BE-RJP-008` |
| Hak akses dari layar Akses Role, tanpa hardcode peran | QBE Strict `PASS`; `[AccessController("OutpatientPriorityReservation")]` dengan aksi `Read`, `Create`, `Complete`, `Screen`, `Cancel` |

## 7. Task berikutnya

1. **Keputusan K-1** — `grill-me` Amendment Pass (owner: Sukma Giri). Bila dipilih opsi (b), lanjut ke
   `plan-module-delivery` untuk satu task `BE` dan satu task `FE`. Task backend menyelesaikan QBE preflight pada saat
   eksekusi dari `AGENTS.md` backend.
2. **Review dan commit** working tree kedua repository (K-6).
3. **Daftar periksa go-live** K-2 s.d. K-5 dan UAT K-7, lalu jalankan ulang `verify-module-readiness` untuk mengubah
   verdict menjadi `READY`.

Tidak ada source aplikasi yang diubah oleh audit ini.
