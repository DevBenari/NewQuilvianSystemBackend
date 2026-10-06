# Laporan Perubahan Backend — `BE-IGD-061`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-061` |
| Judul | Penutupan menyusul dari tiga titik pemicu, dan observasi yang diakhiri sesudah disposisi dilaksanakan |
| Slice | `S5` · `EPIC IGD-13` · `MVP-8` |
| Roadmap | `docs/module-blueprints/igd/roadmap/backend-roadmap.md` bagian R3.14 |
| Trace | `FR-IGD-088`, `092`, `093`; `IGD-DEC-165`, `136`, `171`, `172`, `173`; approval `IGD-DEC-170`, `175` (sementara, `IGD-DEC-174`); `AT-IGD-187`, `188`, `192`, `193` |
| Contract version | validation `0.10.0` §11 aturan 4–7 dan §11.1 aturan 12–15 beserta urutan pemeriksaannya; state `0.7.0` §9.2 dan §9.5; API `0.13.0` §9.1 nomor 4–5 dan §9.4 — `approved`. Hash kelima berkas kontrak cocok dengan manifest 0j, 0j.1, 0j.2 |
| Dependency | `BE-IGD-060` ✅ |
| Klasifikasi | `MEDIUM` — skor 7: repository 0, berkas diperiksa 1, berkas diubah 0 (3 berkas), logika bisnis 2 (penutupan sebagai efek samping, urutan penolakan, transaksi), kontrak API 2 (perilaku `409`/`200` berubah), database 1, keamanan/auth 0, UI/workflow 1 |
| Task mode | `BACKEND` — wewenang pemilik 1 Oktober 2026 (*"Sesudah itu R3.14 (`BE-IGD-061`/`062`/`063`, `FE-IGD-041`/`042`)"*). Tanpa `dotnet build`, migration, `Program.cs`, eksekusi database, commit |
| Target tulis | `NewQuilvianSystemBackend`: source IGD dan `docs/module-blueprints/igd/` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `b9076c71` (`rizkiG`) + working tree `BE-IGD-053`, `057`, `058`, `059` yang belum di-commit |
| Tanggal | 1 Oktober 2026 |
| Status | 🟡 **SEBAGIAN — 4 Oktober 2026 (uji gabungan `MVP-8`).** Uji API S13–S19 pada bukti mentah: **13 terbukti** (S13, S14, S15, S16, S17, S18, dan S19 butir S1, S5, S7, S8, S9, S10, S11 kaki `cancel`); `NOT RUN`: kaki `PUT` S18 (periode `D-S6` tanpa baris pemantauan) dan kaki `reject-handover` S11 (opsional). Acceptance 13–19 terpenuhi; 18 untuk `PUT` hanya lewat pembacaan source. Bukti diterima dengan penyimpangan tercatat (`IGD-DEC-188`: agen penguji mengubah Akses Role Perawat IGD lewat SuperAdmin). **Tetap 🟡:** acceptance 2 dan 3 (S2, S3, S12) tertahan `BE-IGD-039`; acceptance 4 sebagian (kaki kepergian dan pesanan) — bagian *Pemeriksaan bukti uji gabungan `MVP-8` — 4 Oktober 2026*. *Sebelumnya:* 🟡 **SEBAGIAN — 4 Oktober 2026 (build pemilik).** `dotnet build` Rizki 3 Oktober 2026: **0 error, 0 warning** (dilaporkan pemilik 4 Oktober 2026); `QuilvianSystemBackend.dll` 18.55, sesudah edit source terakhir 18.34; source di-commit pemilik sebagai `c1f79f79`. Kriteria 7 terpenuhi. **Belum:** uji API S13–S19 — bagian *Build pemilik — dicatat 4 Oktober 2026*. *Sebelumnya:* 🟡 **SEBAGIAN — 3 Oktober 2026 (pengerjaan ulang, `IGD-DEC-185`).** Penjaga penutupan menghitung observasi `Escalated`; aksi observasi selain *batalkan* dan pemantauan baru pada kunjungan berakhir ditolak `409` dengan kalimat validation `0.11.0` §11.2 aturan 18 dan 21. Tiga berkas source (+32/−5); QBE checker Strict `PASS` (3 berkas, 0 `VIOLATION`, 0 `REVIEW`). Acceptance 13–18 terpetakan ke source. **Belum:** `dotnet build` pemilik (kriteria 7) dan uji API S13–S19 — bagian *Pengerjaan ulang 3 Oktober 2026*. *Sebelumnya:* 🟡 **SEBAGIAN — 2 Oktober 2026.** Build pemilik terbukti. Uji API: **6 terbukti** (S1, S5, S7, S8, S9, S10), **2 sebagian** (S4, S11), **4 belum terbukti** (S2, S3, S6, S12). Pemicu lewat observasi dan lewat pembatalan kepergian terbukti; pemicu `accept-handover`, `reject-handover`, dan sikap pesanan **belum** — tertahan `403` kewenangan unit sebelum sampai ke kode task ini. Kriteria 8 (observasi Dieskalasi pada kunjungan `Disposed`) belum terbukti lewat API. *Sebelumnya:* 🟡 **SEBAGIAN — Implementation Complete.** Tiga berkas source; QBE checker `PASS`. **Belum:** build pemilik dan uji API S1–S12 (bagian 5.1) |

### Backend Governance Preflight

| Butir | Hasil |
| --- | --- |
| Governance terbaca | `AGENTS.md` backend; `rules/backend/` suite 1.17.1; kontrak rekayasa dan registry |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Owner / prefix registry | Emergency, `Emg`, `ACTIVE / LEGACY`. Nol entity baru |
| Keberlakuan | `NEW CODE`: `EmergencyVisitService.TryCloseAfterBlockerResolvedAsync`. `TOUCHED LEGACY`: `EmergencyObservationController.UpdateObservationStatus`, enam action `EmergencyDepartureController` |
| Branch | `rizkiG`, upstream `origin/rizkiG` |
| QBE yang berlaku | `QBE-SVC-001` (orkestrasi transaksi dan penyimpanan di service; controller kepergian tetap tanpa context), `QBE-TXN-001` (aksi pemicu dan penutupan dalam satu transaksi), `QBE-API-001`, `QBE-PERM-001` (metadata akses tidak berubah), `QBE-VAL-001`, `QBE-LOG-001` (pelaku penutupan tercatat) |
| Tidak berlaku | `QBE-ENT-*`, `QBE-NAM-*`, `QBE-CFG-*`, `QBE-MOD-*`, `QBE-CODE-*`, `QBE-DTO-001` (bentuk request dan response tidak berubah) |
| Checker QBE | Strict, working tree: 0 `VIOLATION`, 0 `REVIEW`, `PASS` |

---

## 1. Masalah yang diperbaiki

Sejak `BE-IGD-060`, menandai tindak lanjut pasien *dilaksanakan* menutup kunjungannya — kecuali masih ada kewajiban
klinis yang belum tuntas. Dua celah tersisa:

| Celah | Akibat |
| --- | --- |
| Kunjungan yang tertahan tidak pernah tertutup sendiri | Sesudah kewajiban terakhir dibereskan, kunjungan tetap terbuka sampai ada yang ingat menekan *selesaikan* |
| Observasi yang masih berjalan sesudah tindak lanjut dilaksanakan **tidak dapat diselesaikan** | Menyelesaikannya ditolak `409` (kunjungan `Disposed` tidak boleh mundur); satu-satunya jalan membatalkan observasi, sehingga kesimpulannya hilang |

*Contoh.* Pasien pulang pukul 14.00, tindak lanjutnya ditandai dilaksanakan, tetapi observasinya lupa ditutup. Pukul
15.00 perawat menyelesaikan observasi dengan kesimpulan *"tanda vital stabil"*. Sebelum task ini: `409`. Sesudahnya:
kesimpulan tersimpan dan kunjungan tertutup saat itu juga atas nama perawat pukul 15.00.

---

## 2. Proses bisnis

**Pelaku.** Petugas yang membereskan kewajiban terakhir — perawat (observasi), perawat unit penerima (serah
terima), atau petugas yang menetapkan sikap pesanan.

**Tiga titik pemicu.** Sesudah aksinya tersimpan, sistem mencoba menutup kunjungan:

| # | Aksi | Endpoint |
| ---: | --- | --- |
| 1 | Observasi diselesaikan atau dibatalkan pada kunjungan `Disposed` | `PATCH /emergency-observations/{id}/observation-status` |
| 2 | Serah terima diterima, ditolak, atau kepergian dibatalkan | `POST …/accept-handover`, `POST …/reject-handover`, `PATCH …/cancel` |
| 3 | Sikap atas pesanan ditetapkan, pesanan diterima atau ditolak | `PATCH …/order-items/{itemId}/action`, `POST …/accept`, `POST …/reject` |

**Urutan pada tiap pemicu.**

1. Aksi petugas divalidasi dan disimpan seperti biasa.
2. Bila kunjungannya punya tindak lanjut yang sudah dilaksanakan dan belum selesai, penjaga penutupan yang sudah ada
   dijalankan — tanpa diubah.
3. Penjaga lolos → kunjungan `Completed`, encounter ikut tertutup, penanda asal penutupan menunjuk tindak lanjut itu,
   pelakunya petugas yang baru saja beraksi.
4. Penjaga menolak (masih ada penahan lain) → aksi petugas **tetap berhasil**; kunjungan tetap menunggu.
5. Langkah 1–3 berada dalam **satu transaksi**: bila penutupan gagal disimpan, aksi petugas ikut batal.

**Observasi pada kunjungan yang tindak lanjutnya sudah dilaksanakan.**

| Tujuan | Yang terjadi |
| --- | --- |
| *Selesaikan* (dari aktif maupun dari tereskalasi) | Diterima. Status kunjungan tidak dipindahkan. Kesimpulan disimpan, lalu penutupan dicoba |
| *Eskalasi* | **Ditolak `409`** *"Tindak lanjut pasien sudah dilaksanakan; eskalasi tidak dapat dicatat pada kunjungan ini."* — observasi tetap aktif dan tetap menahan penutupan |
| *Batalkan* | Seperti semula, ditambah percobaan penutupan |
| *Aktifkan* | Seperti semula — ditolak penjaga kunjungan dengan pesannya sendiri |

**Urutan penolakan observasi:** `404` → `400` perpindahan status observasi tidak sah → `409` eskalasi pada kunjungan
`Disposed` → `400` catatan lebih dari 1000 karakter → simpan → penutupan.

*Contoh urutan.* Eskalasi dengan catatan 1.200 karakter pada kunjungan `Disposed` dijawab `409`, bukan `400`:
walaupun catatannya diringkas, eskalasinya tetap ditolak.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `BE-IGD-061`; validation §11, §11.1; state §9.2, §9.5; API §9.1, §9.4.
Source: `EmergencyObservationController.cs`, `EmergencyDepartureController.cs`, `EmergencyDepartureService.cs`
(`UpdateHandoverAsync`, `CancelAsync`, `SetOrderActionAsync`, `SetOrderAcceptanceAsync`, `UbahFisikAsync`,
`UbahHandoverAsync`), `EmergencyDispositionService.ValidateVisitClosureAsync`, `EmergencyVisitService.cs`
(`TryCloseAfterDispositionAsync`, `ApplyEncounterClosureAsync`), `EmergencyDispositionController.UpdateDispositionStatus`
(pola `BE-IGD-060`), `ClinicalDocumentIntegrityService`, `EmgDeparture.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `…/Controllers/EmergencyObservationController.cs` | Penolakan eskalasi pada kunjungan `Disposed` sebelum pemeriksaan lain; *selesaikan* pada kunjungan `Disposed` tidak lagi memetakan ke `AwaitingDisposition`; sesudah observasi tersimpan, penutupan dicoba di dalam transaksi yang sama; log memuat hasil penutupan. +58/−3 baris |
| `…/Controllers/EmergencyDepartureController.cs` | Enam action — `accept-handover`, `reject-handover`, `cancel`, `order-items/{itemId}/action`, `accept`, `reject` — dijalankan lewat pembantu `DenganPenutupanSusulanAsync`; constructor + `EmergencyVisitService`; log penutupan. +99/−8 baris |
| `…/Services/EmergencyVisitService.cs` | + `TryCloseAfterBlockerResolvedAsync`: membuka transaksi, menjalankan aksi pemicu, mencoba penutupan, menyimpan, lalu menutup transaksi. +27 baris |

**Nol baris komentar ditambah** — jumlah baris komentar ketiga berkas sama dengan sebelum disunting. **Tidak
disentuh:** `EmergencyObservationService`, `EmergencyDepartureService`, `EmergencyDispositionService`, penjaga
penutupan, `Program.cs`, model, configuration, migration, frontend.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sesuai kontrak terkunci (API §9.1 nomor 4–5, §9.4). Bentuk request dan response tidak berubah. `PATCH observation-status` pada kunjungan `Disposed`: *selesaikan* kini `200` (dulu `409`); *eskalasi* tetap `409` dengan kalimat baru |
| Database | Nol schema, nol migration. Penutupan menulis kolom yang sama dengan `BE-IGD-060` |
| Keamanan/Auth | Nol hak akses baru: petugas menutup kunjungan sebagai akibat aksi yang memang wewenangnya (permission §8) |

### 3.4 Selisih terhadap kartu dan keputusan pelaksanaan

| Butir | Isi |
| --- | --- |
| **Pemicu dijalankan sesudah aksi tersimpan, bukan sebelum `SaveChanges`** | Kartu menulis *"titik pemicu … sebelum `SaveChanges`"*. Penjaga penutupan membaca **basis data**, bukan perubahan yang masih di memori; observasi yang baru diselesaikan tetapi belum disimpan masih terbaca aktif, sehingga penjaga selalu menolak dan penutupan tidak pernah terjadi. Urutannya karena itu: simpan aksi → jalankan penjaga → simpan penutupan, seluruhnya dalam satu transaksi — memenuhi validation §11.1 aturan 13 (*"pada penyimpanan yang sama"*) |
| **`EmergencyVisitService` ikut disentuh** | Kartu menulis berkas itu tidak disentuh. Controller kepergian tidak memegang context basis data, dan memasukkannya membuat checker QBE memberi dua catatan `QBE-SVC-001`. Layanan kepergian tidak dapat memanggil layanan kunjungan (lingkaran dependensi: kunjungan → disposisi → kepergian). Satu method ditambahkan pada layanan kunjungan, yang sudah terdaftar — nol `Program.cs` |
| Pada observasi, penutupan hanya dicoba bila kunjungannya `Disposed` | Penjaga penutupan memang mensyaratkan `Disposed`, jadi hasilnya sama; jalur biasa tidak terbebani transaksi dan kueri tambahan (kriteria 4 dan 12) |
| Enam aksi kepergian kini berjalan di dalam transaksi | Sebelumnya masing-masing satu penyimpanan tanpa transaksi. Bila aksinya ditolak, transaksi dibatalkan — tidak ada yang tersimpan sebelum penolakan pada keenam jalur itu, jadi perilaku penolakannya tidak berubah |
| **`arrive` tidak dipasangi pemicu** | Kontrak (state §9.2, validation §11 aturan 4) menyebut tiga aksi serah terima, dan kartu mengikutinya. Padahal penahan *"masih ada proses kepergian yang belum selesai"* dilepas oleh **keadaan fisik** pasien (`arrive` atau `cancel`), bukan oleh serah terima. Akibatnya: bila serah terima diterima **sebelum** pasien dicatat tiba, pencatatan tiba sesudahnya tidak menutup kunjungan. Kunjungan itu tetap dapat ditutup manual dan tetap tampil pada saringan menunggu penutupan. Menambah `arrive` sebagai pemicu adalah perubahan kontrak — keputusan pemilik |

---

## 4. Dokumentasi endpoint

Seluruhnya endpoint yang sudah ada; yang bertambah hanya efek samping penutupan kunjungan.

#### Health Services / Emergency Installation Management / Emergency Observation

Base URL: `api/v1/health-services/emergency-installation-management/emergency-observations`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/{id}/observation-status` | Mengubah status observasi; pada kunjungan `Disposed`: *selesaikan* diterima dan penutupan dicoba, *eskalasi* ditolak `409` | `EmergencyObservation : Update` (tidak berubah) |

#### Health Services / Emergency Installation Management / Emergency Departure

Base URL: `api/v1/health-services/emergency-installation-management/emergency-departures`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id}/accept-handover`, `/{id}/reject-handover` | Meninjau serah terima; penutupan kunjungan dicoba | `EmergencyDeparture : Update` (tidak berubah) |
| `PATCH` | `/{id}/cancel` | Membatalkan kepergian; penutupan kunjungan dicoba | `EmergencyDeparture : Update` (tidak berubah) |
| `PATCH` | `/{id}/order-items/{itemId}/action` | Menetapkan sikap pesanan; penutupan kunjungan dicoba | `EmergencyDeparture : Update` (tidak berubah) |
| `POST` | `/{id}/order-items/{itemId}/accept`, `/reject` | Menerima atau menolak pesanan; penutupan kunjungan dicoba | `EmergencyDeparture : Update` (tidak berubah) |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE checker Strict (working tree) | 0 `VIOLATION`, 0 `REVIEW`, `Final result: PASS` | `PASS` | Keluaran perintah 1 Oktober 2026 |
| Review diff | Hanya perubahan task; akhiran baris tiap berkas dipertahankan; nol baris komentar ditambah | `PASS` | `git diff`; hitungan baris komentar sebelum dan sesudah sama |
| Lingkaran dependensi | Nol: controller kepergian → layanan kunjungan → layanan disposisi → layanan kepergian | `PASS` | Baca constructor ketiga layanan |
| `dotnet build` | — | `NOT RUN` | Milik pemilik |
| Uji API S1–S12 | — | `NOT RUN` | Menunggu build pemilik |

Uji manual: `REQUIRED`.

**Tidak dijalankan:** `dotnet build` (arahan pemilik), uji API (butuh build), eksekusi database.

### 5.1 Skenario uji untuk pemilik

Build: `dotnet build ./QuilvianSystemBackend.sln -p:RunAnalyzers=false`. Simpan **badan respons** tiap langkah.
"Menunggu penutupan" = kunjungan `Disposed` yang tindak lanjutnya `Executed` tetapi masih punya penahan.

| # | Skenario | Hasil yang diharapkan | Kriteria |
| ---: | --- | --- | ---: |
| S1 | Kunjungan menunggu penutupan dengan **satu observasi aktif** sebagai satu-satunya penahan. `PATCH observation-status` `{ "observationStatus": 2, "notes": "tanda vital stabil" }` | `200`; observasi `Completed`, `completionSummary` terisi. `GET` kunjungan: `Completed`, `closedByDispositionId` terisi tindak lanjut itu, pelaku pembaruan = pengguna yang menyelesaikan observasi; encounter tertutup | 1 |
| S2 | Kunjungan menunggu penutupan karena kepergian yang belum tuntas: catat pasien tiba (`arrive`), lalu `POST accept-handover` | Sesudah `accept-handover`: `200`; kunjungan `Completed` | 2 |
| S3 | Kunjungan menunggu penutupan karena satu pesanan belum bersikap: `PATCH order-items/{itemId}/action` | `200`; kunjungan `Completed` | 3 |
| S4 | Aksi yang sama dengan S1–S3 pada kunjungan yang **tidak** menunggu penutupan (misalnya masih `InTreatment`) | Aksinya berhasil seperti biasa; status kunjungan mengikuti aturan lama; tidak tertutup | 4, 12 |
| S5 | Kunjungan menunggu penutupan dengan **dua** penahan (observasi aktif + pesanan belum bersikap): selesaikan observasinya | `200`; observasi `Completed`; kunjungan tetap `Disposed` | 5 |
| S6 | Observasi berstatus `Escalated` pada kunjungan `Disposed`: selesaikan | `200`; kunjungan tidak dipindahkan ke `AwaitingDisposition`; tertutup bila tak ada penahan lain | 8 |
| S7 | Kunjungan `Disposed` dengan observasi aktif: `{ "observationStatus": 3, "notes": "pasien memburuk" }` (eskalasi) | `409` *"Tindak lanjut pasien sudah dilaksanakan; eskalasi tidak dapat dicatat pada kunjungan ini."*; observasi tetap `Active`, `escalationReason` kosong, kunjungan tetap `Disposed` | 9 |
| S8 | S7 dengan catatan 1.200 karakter | `409` kalimat yang sama, **bukan** `400` | 10 |
| S9 | Kunjungan `Disposed`: selesaikan observasi dengan catatan 1.200 karakter | `400` *"Catatan paling banyak 1000 karakter."*; observasi dan kunjungan tidak berubah | 11 |
| S10 | Kunjungan `UnderObservation`: selesaikan observasi; kunjungan `InTreatment` lain: eskalasi observasi | Kunjungan pertama → `AwaitingDisposition`; kedua → tetap lewat penjaga seperti semula | 12 |
| S11 | Kunjungan menunggu penutupan: `POST reject-handover` (beralasan), dan pada kunjungan lain `PATCH cancel` kepergian | Aksi `200`; kunjungan tertutup bila itu penahan terakhir | 2, 6 |
| S12 | Kunjungan menunggu penutupan: `POST order-items/{itemId}/accept`, dan pada pesanan lain `…/reject` | Aksi `200`; kunjungan tertutup bila itu penahan terakhir | 3, 6 |

Nilai `observationStatus`: `Active` 1, `Completed` 2, `Escalated` 3, `Cancelled` 4.

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Observasi diselesaikan dengan kesimpulan → `200`, kunjungan `Completed`, pelakunya petugas itu, `ClosedByDispositionId` terisi | **Terpenuhi pada source** | `UpdateObservationStatus`. Uji S1 belum |
| 2 | Serah terima diterima → kunjungan tertutup | **Terpenuhi pada source** | `AcceptHandover` lewat `ExecuteDeparture(…, true)`. Uji S2 belum |
| 3 | Sikap pesanan ditetapkan → kunjungan tertutup | **Terpenuhi pada source** | `SetOrderAction`, `SetOrderAcceptance`. Uji S3 belum |
| 4 | Aksi yang sama pada kunjungan yang tidak menunggu penutupan tidak berefek | **Terpenuhi pada source** | `TryCloseAfterDispositionAsync` melewati kunjungan tanpa tindak lanjut `Executed`. Uji S4 belum |
| 5 | Masih ada penahan lain → kunjungan tetap menunggu; observasi tetap `Completed` | **Terpenuhi pada source** | Penjaga menolak → hanya aksi yang disimpan. Uji S5 belum |
| 6 | Ketiga titik pemicu benar-benar dipasang — nol tertinggal | **Terpenuhi** | Satu action observasi + enam action kepergian; `git diff`. `arrive` sengaja tidak — lihat 3.4 |
| 7 | Build 0 error | **Belum** | Build milik pemilik |
| 8 | Observasi `Escalated` pada kunjungan `Disposed` diselesaikan → `200`, kunjungan tetap `Disposed` sebelum penutupan dicoba | **Terpenuhi pada source** | Pemetaan `Completed when !kunjunganDisposed`. Uji S6 belum |
| 9 | Eskalasi pada kunjungan `Disposed` → `409` kalimat aturan 14 persis; nol perubahan | **Terpenuhi pada source** | Penolakan sebelum entity disentuh; kalimat sama huruf demi huruf dengan validation §11.1 aturan 14. Uji S7 belum |
| 10 | Eskalasi dengan catatan 1.200 karakter → `409`, bukan `400` | **Terpenuhi pada source** | Penolakan eskalasi mendahului pemeriksaan catatan. Uji S8 belum |
| 11 | Selesaikan dengan catatan 1.200 karakter pada kunjungan `Disposed` → `400` | **Terpenuhi pada source** | Tanpa target status kunjungan, pemeriksaan batas catatan berlaku. Uji S9 belum |
| 12 | Perilaku pada kunjungan selain `Disposed` tidak berubah | **Terpenuhi pada source** | Seluruh cabang baru berada di balik `kunjunganDisposed`. Uji S10 belum |

DoD: laporan tracked ✅ (berkas ini). Belum ✅ karena kriteria 7 belum ada dan uji API belum dijalankan. `FE-IGD-042`
boleh **dikerjakan**; uji layarnya menunggu build task ini.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Penutupan terjadi sebagai efek samping. Pemanggil keenam endpoint kepergian dan endpoint observasi dapat mendapati kunjungan `Completed` pada pembacaan berikutnya |
| Masalah yang diketahui | (1) `arrive` bukan titik pemicu (3.4). (2) Observasi `Escalated` dapat tertinggal pada kunjungan `Completed` dan hanya dapat dibatalkan — di luar cakupan kartu (state §9.5). (3) Cara mencatat pasien yang memburuk sesudah tindak lanjutnya dilaksanakan belum diputuskan (`IGD-OQ-111`) |
| Risiko tersisa | Belum ada build. Enam aksi kepergian kini di dalam transaksi — bila salah satu method layanan kepergian kelak membuka transaksinya sendiri, keduanya akan bertabrakan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Tiga berkas source berubah oleh task ini (`EmergencyVisitService.cs` bersama perubahan `BE-IGD-057`/`058`); laporan ini; status roadmap/traceability. Tanpa stage, commit, atau push |
| Langkah berikutnya | Pemilik: build, lalu S1–S12. Agent: `BE-IGD-062`, `063`, lalu `FE-IGD-042` |

---

## Pemeriksaan bukti uji gabungan — 2 Oktober 2026

Bukti mentah di `QuilvianSystemFrontendDev/test-with-agy/igd/` (`results-tahap-1.json`…`results-tahap-4.json`, skrip `test-tahap-*.mjs`, tangkapan layar `<ID>.png`) dan log backend `Logs/quilvian-backend-20261001.json`, `quilvian-backend-20261002.json`. Ringkasan agen penguji ([laporan uji gabungan](../../../testing/2026-10-01-laporan-uji-gabungan-r313-r314.md)) **tidak** dipakai sebagai bukti: uraian skenarionya pada beberapa task tidak sama dengan panduan, dan daftar `FAIL`-nya tidak cocok dengan JSON mentah.

Build pemilik terbukti dari artefak: `bin/Debug/net9.0/QuilvianSystemBackend.dll` bertanggal 1 Oktober 2026 15.18, sesudah edit source terakhir (14.54); jumlah warning tidak dilaporkan. Source di-commit pemilik sebagai `74a72399`.

| Skenario | Putusan | Yang teramati pada bukti mentah |
| --- | --- | --- |
| `061-S1` | **Terbukti** | Observasi `200` → `Completed`, `completionSummary` terisi; kunjungan `visitStatus` 9; encounter `encounterStatus` 9 |
| `061-S2` | **Belum terbukti** | `arrive` dan `accept-handover` keduanya `403` *"Unit Rawat Inap belum dipetakan ke simpul organisasi…"*; kunjungan tetap 7. Pemicu tidak pernah tercapai |
| `061-S3` | **Belum terbukti** | Skrip gagal membuat pesanan: `POST order-items` `403` *"Unit asal belum tercatat…"*; `PATCH …/action` tidak pernah dikirim |
| `061-S4` | **Sebagian** | Hanya kaki observasi: selesai pada kunjungan `InTreatment` → `200`, kunjungan menjadi 6 (aturan lama), tidak tertutup. Kaki kepergian dan pesanan tidak dijalankan |
| `061-S5` | **Terbukti** | Dua penahan (observasi + kepergian): observasi `200`, kunjungan tetap 7 dengan alasan kepergian |
| `061-S6` | **Belum terbukti** | Prasyarat tidak terpenuhi. Observasi dieskalasi saat `InTreatment`, lalu tindak lanjut dijalankan: kunjungan **langsung `Completed`** karena periode Dieskalasi tidak menahan penutupan. Penyelesaian periode itu sesudahnya `409` *"Status kunjungan tidak dapat berubah dari Completed ke AwaitingDisposition."* |
| `061-S7` | **Terbukti** | `409` dengan kalimat `IGD-DEC-172` |
| `061-S8` | **Terbukti** | Catatan 1.200 karakter tetap `409` kalimat yang sama, bukan `400` |
| `061-S9` | **Terbukti** | `400` *"Catatan paling banyak 1000 karakter."* |
| `061-S10` | **Terbukti** | Kunjungan `UnderObservation` → 6; eskalasi pada `InTreatment` `200`. Bukti tipis: hanya nilai ringkas, tanpa badan respons |
| `061-S11` | **Sebagian** | `PATCH cancel` kepergian `200` → kunjungan `Completed` 9. Kaki `reject-handover` tidak dijalankan |
| `061-S12` | **Belum terbukti** | Sama dengan S3: `POST order-items` `403`; `accept` dan `reject` tidak pernah dikirim |

**Catatan.**

- **Bukan kegagalan kode task ini.** `403` berasal dari pemeriksaan kewenangan unit (`EmergencyUnitAuthorityService`, `IGD-DEC-092`): unit tujuan belum dipetakan ke simpul organisasi dan kepergian uji dibuat tanpa unit asal. Skenario baru dapat dibuktikan pada data yang unitnya terpetakan dan penggunanya ditugaskan ke unit itu.
- **Temuan untuk pemilik (aturan lama, bukan dari task ini).** Penjaga penutupan hanya menghitung observasi `Active`. Periode `Escalated` tidak menahan, sehingga kunjungan dapat `Completed` sementara periodenya masih Dieskalasi, dan periode itu tidak dapat diselesaikan lagi (`409`). `closedByDispositionId` tidak tampil pada respons `GET`, jadi tidak teramati.
- Kriteria 8 pada layar: `042-U6` memperlihatkan periode Dieskalasi pada kunjungan `Disposed` (ada penahan lain) beserta modalnya; hasil penyelesaiannya tidak dijalankan.

Putusan: **🟡 sebagian** — Build pemilik terbukti. Uji API: **6 terbukti** (S1, S5, S7, S8, S9, S10), **2 sebagian** (S4, S11), **4 belum terbukti** (S2, S3, S6, S12). Pemicu lewat observasi dan lewat pembatalan kepergian terbukti; pemicu `accept-handover`, `reject-handover`, dan sikap pesanan **belum** — tertahan `403` kewenangan unit sebelum sampai ke kode task ini. Kriteria 8 (observasi Dieskalasi pada kunjungan `Disposed`) belum terbukti lewat API.

---

## Pengerjaan ulang — 3 Oktober 2026: observasi Dieskalasi dan kunjungan yang sudah berakhir

| Field | Nilai |
| --- | --- |
| Pemicu | Temuan S6 (2 Oktober 2026): kunjungan tertutup sementara observasinya masih Dieskalasi, lalu observasi itu tidak dapat diselesaikan lagi |
| Keputusan | `IGD-DEC-183` (observasi Dieskalasi menahan penutupan), `IGD-DEC-184` (data lama dibiarkan; pesan pada kunjungan berakhir diperjelas), `IGD-DEC-185` (dikerjakan di kartu ini), `IGD-DEC-186` (approval kontrak, termasuk aturan 21 pemantauan); asumsi `IGD-ASM-003` |
| Contract version | validation `0.11.0` §6 aturan 2, §9.1 langkah 2a, §11.2 aturan 16–21 beserta urutan pemeriksaannya; state `0.8.0` §9.6; API `0.14.0` §9.1 nomor 6–8 dan §9.4 — `approved` (`IGD-DEC-186`). Hash ketiga berkas kontrak cocok dengan manifest bagian 0k |
| Kartu | `backend-roadmap.md` R3.14 — perluasan 3 Oktober 2026, acceptance 13–19 |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 1, berkas diubah 0 (3 berkas), logika bisnis 2 (penjaga penutupan berlaku di tiga jalur; urutan penolakan), kontrak API 2 (`409` baru pada `complete` dan pemantauan; pesan `409` berganti), database 0, keamanan/auth 0, UI/workflow 1 |
| Task mode | `BACKEND` — perintah pemilik 3 Oktober 2026: *"…lalu plan-module-delivery dan build-module-backend untuk BE-IGD-061"*. Tanpa `dotnet build` (milik Rizki), migration, `Program.cs`, eksekusi database, commit |
| Target tulis | `NewQuilvianSystemBackend`: source IGD dan `docs/module-blueprints/igd/` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `2a63a3bb` (`rizkiG`, `ahead 11`); source IGD identik dengan `5af6ef3b`, working tree source bersih sebelum dikerjakan |
| Tanggal | 3 Oktober 2026 |

### Backend Governance Preflight

| Butir | Hasil |
| --- | --- |
| Governance terbaca | `AGENTS.md` backend; `rules/backend/` suite 1.19.2 (`TASK_RULES`, `TEST_POLICY`, `REPORT_TEMPLATE`); `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` dan registry |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Owner / prefix registry | Emergency, `Emg`, `ACTIVE / LEGACY`. Nol entity baru |
| Keberlakuan | `TOUCHED LEGACY`: `EmergencyDispositionService.ValidateVisitClosureAsync`, `EmergencyObservationController.UpdateObservationStatus`, `EmergencyObservationService.ValidateDetailScopeAsync` |
| Branch | `rizkiG`, upstream `origin/rizkiG` |
| QBE yang berlaku | `QBE-API-001` (envelope `ApiResponse`, `409` mengikuti pola penolakan yang sudah ada), `QBE-VAL-001` (invarian kunjungan berakhir), `QBE-PERM-001` (metadata akses tidak berubah), `QBE-SVC-001` (aturan pemantauan diletakkan di service yang sudah memegang urutan pemeriksaan) |
| Tidak berlaku | `QBE-ENT-*`, `QBE-NAM-*`, `QBE-CFG-*`, `QBE-MOD-*`, `QBE-CODE-*`, `QBE-DTO-001` (bentuk request/response tidak berubah), `QBE-TXN-001` (nol tulisan baru), `QBE-LOG-001` (penolakan tidak mengubah state) |
| Checker QBE | Strict, working tree: 3 berkas dinilai, 0 `VIOLATION`, 0 `REVIEW`, 0 `INFO`, `PASS` |

### Masalah dan proses bisnis

Penjaga penutupan hanya menghitung observasi Aktif, padahal periode Dieskalasi belum ditutup menurut `IGD-DEC-126` —
periode itu masih menerima pemantauan. Akibatnya:

1. Perawat mengeskalasi observasi saat pasien ditangani; kunjungan kembali *Sedang Ditangani*.
2. Dokter memutuskan pasien pulang; perawat menandai tindak lanjut *dilaksanakan*.
3. **Dulu:** kunjungan langsung *Selesai*, observasi tetap *Dieskalasi* tanpa kesimpulan, dan sesudahnya menekan
   *Selesaikan* dijawab pesan teknis *"Status kunjungan tidak dapat berubah dari Completed ke AwaitingDisposition."*
4. **Sekarang:** kunjungan menunggu penutupan dengan alasan *"Masih ada observasi yang belum diselesaikan."* Perawat
   menyelesaikan periode yang dieskalasi beserta kesimpulannya — atau membatalkannya bila salah dibuka — dan kunjungan
   tertutup pada penyimpanan itu atas nama perawat tersebut.

*Contoh.* Pukul 10.00 observasi Pak Budi dieskalasi karena saturasi turun. Pukul 13.00 disposisi pulang dilaksanakan;
kunjungan tampil *Menunggu penutupan — Masih ada observasi yang belum diselesaikan.* Pukul 13.10 perawat menyelesaikan
observasi pukul 10.00 dengan kesimpulan *"membaik sesudah penanganan, layak pulang"*, dan kunjungan tertutup.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Tombol *Selesaikan kunjungan* ditekan saat masih ada observasi Dieskalasi | `409` *"Masih ada observasi yang belum diselesaikan."* — sama dengan observasi Aktif |
| Kunjungan sudah *Selesai* atau *Batal*, perawat menyelesaikan, mengeskalasi, atau mengaktifkan observasinya | `409` *"Kunjungan IGD ini sudah berakhir; observasinya tidak dapat diselesaikan, dieskalasi, atau diaktifkan kembali."* — observasi tidak berubah, walaupun catatannya melebihi 1000 karakter |
| Kunjungan sudah berakhir, perawat membatalkan observasinya | `200` seperti sebelumnya; status kunjungan tidak berubah dan penutupan tidak dicoba |
| Kunjungan sudah berakhir, perawat menambah pemantauan pada periode mana pun | `409` *"Kunjungan IGD ini sudah berakhir; pemantauan observasi tidak dapat ditambahkan lagi."* — nol baris tersimpan |
| Observasi Dieskalasi yang sudah tertinggal pada kunjungan selesai sebelum perubahan ini | Dibiarkan (`IGD-DEC-184`); jumlahnya dihitung pemilik per lingkungan (`IGD-OQ-112`) |

### Berkas yang diperiksa dan berubah

Diperiksa: `EmergencyDispositionService.cs` (`ValidateVisitClosureAsync`), `EmergencyObservationController.cs`
(`UpdateObservationStatus`, konstanta pesan), `EmergencyObservationService.cs` (`ValidateDetailScopeAsync`, konstanta
pesan), `EmergencyObservationDetailController.cs` (`POST` — meneruskan `StatusCode` hasil pemeriksaan lewat `Failure`),
`EmergencyVisitService.cs` (`EpisodeMasihBerjalan`, `SaringMenungguPenutupan`, `AmbilAlasanMenungguPenutupanAsync`),
`EmgVisit.cs`, `EmgObservation.cs`.

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDispositionService.cs` | Penjaga penutupan menghitung `Active` **atau** `Escalated`; variabel `adaObservasiAktif` → `adaObservasiBelumSelesai`; kalimat tidak berubah. +6/−3 (2 baris komentar) |
| `Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyObservationController.cs` | Konstanta `PesanObservasiPadaKunjunganBerakhir`; penolakan `409` bila kunjungan tidak lagi berjalan (`EmergencyVisitService.EpisodeMasihBerjalan`) dan target bukan `Cancelled`, sesudah pemuatan kunjungan dan sebelum pemeriksaan eskalasi. +9 (2 baris komentar) |
| `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyObservationService.cs` | Konstanta `PesanKunjunganBerakhir`; proyeksi periode + `VisitStatus`; langkah 2a sesudah penolakan periode tertutup, hanya bila `tolakPeriodeTertutup` (`POST`); dokumentasi parameter diperbarui. +17/−2 (5 baris komentar) |

Nol `Program.cs`, nol model, nol configuration, nol migration, nol DTO, nol metadata akses. Akhiran baris CRLF
dipertahankan. `EmergencyObservationDetailController` tidak disentuh — penolakan baru mengalir lewat jalur
`Failure(pemeriksaan.StatusCode, …)` yang sudah ada.

### Keputusan pelaksanaan

| Keputusan | Alasan |
| --- | --- |
| Arti "kunjungan berakhir" memakai `EmergencyVisitService.EpisodeMasihBerjalan` yang sudah ada | Satu definisi untuk `Completed`/`Cancelled`, sama dengan yang dipakai `BE-IGD-062` dan penutupan susulan; tidak ditulis ulang |
| Aturan 21 di `ValidateDetailScopeAsync`, bukan di controller | Urutan validation §9.1 mengikat: periode tertutup → kunjungan berakhir → tautan tanda vital. Pemeriksaan di controller sesudah method kembali akan membuat penolakan tautan dijawab lebih dulu (kartu R3.14, perluasan c) |
| Status kunjungan diproyeksikan pada kueri periode yang sudah ada | Nol kueri tambahan; navigasi `EmergencyVisit` sudah dipakai untuk `PatientId` dan `EncounterId` |
| Periode tanpa kunjungan (`EmergencyVisit` kosong) tidak ditolak oleh langkah 2a | Mengikuti perlakuan proyeksi yang sudah ada; keadaan itu bukan "kunjungan berakhir" |
| Nol perubahan pada saringan menunggu penutupan | Saringan membaca status kunjungan dan disposisi `Executed`; kunjungan yang kini tertahan otomatis tetap `Disposed` dan ikut tersaring, dengan alasan dari penjaga yang sama |

### Dokumentasi endpoint

#### Health Services / Emergency Installation Management / Emergency Visit

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/v1/health-services/emergency-installation-management/emergency-visits/{id}/complete` | Menyelesaikan kunjungan secara manual — kini `409` *"Masih ada observasi yang belum diselesaikan."* juga bila ada observasi Dieskalasi | Tidak berubah |
| `GET` | `/v1/health-services/emergency-installation-management/emergency-visits?awaitingClosure=true` | Daftar menunggu penutupan — kini memuat kunjungan yang tertahan observasi Dieskalasi, dengan alasan observasi | `EmergencyVisit : Read` |

#### Health Services / Emergency Installation Management / Emergency Observation

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/v1/health-services/emergency-installation-management/emergency-observations/{id}/observation-status` | Mengubah status observasi — pada kunjungan berakhir, target selain `Cancelled` dijawab `409` dengan kalimat aturan 18 | `EmergencyObservation : Update` |

#### Health Services / Emergency Installation Management / Emergency Observation Detail

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/emergency-installation-management/emergency-observation-details` | Mencatat pemantauan — pada kunjungan berakhir dijawab `409` dengan kalimat aturan 21 | `EmergencyObservationDetail : Create` |

Nol endpoint baru; bentuk request dan response tidak berubah.

### Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE checker Strict, working tree | 3 berkas, 0 `VIOLATION`, 0 `REVIEW`, 0 `INFO` | `PASS` | Keluaran `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` |
| Review diff dan cakupan | Tiga berkas source IGD; nol berkas di luar cakupan kartu | `PASS` | `git diff --stat -- Areas/HealthServices/EmergencyInstallationManagement` |
| Kalimat pesan sama persis dengan kontrak | Aturan 18 dan 21 disalin dari validation `0.11.0` §11.2 | `PASS` | Perbandingan teks |
| Akhiran baris | CRLF di ketiga berkas | `PASS` | Pemeriksaan byte |
| `dotnet build` (Rizki, 3 Oktober 2026) | 0 error, 0 warning | `PASS` | Dilaporkan pemilik 4 Oktober 2026; `bin/Debug/net9.0/QuilvianSystemBackend.dll` 18.55, sesudah edit source terakhir 18.34 |
| Uji API acceptance 13–19 | — | `NOT RUN` | Milik pemilik, sesudah build — skenario di bawah |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `REQUIRED` — acceptance 13–19 lewat API pada dev, sesudah build pemilik.

#### Skenario uji API untuk pemilik

Pakai akun dengan izin `EmergencyVisit`, `EmergencyObservation`, `EmergencyObservationDetail`, dan
`EmergencyDisposition`. Kunjungan uji S6 2 Oktober (sudah `Completed`, observasinya masih `Escalated`) dipakai untuk
S17 dan S18 — **jangan** dibatalkan observasinya sebelum kedua skenario itu dijalankan. Nilai `observationStatus`:
`Active` 1, `Completed` 2, `Escalated` 3, `Cancelled` 4.

| # | Langkah | Yang diharapkan | Kriteria |
| ---: | --- | --- | ---: |
| S13 | Kunjungan baru: buat observasi, eskalasi saat `InTreatment`; tetapkan dan jalankan disposisi | Disposisi `Executed`; kunjungan **tetap** `Disposed`; `GET /emergency-visits?awaitingClosure=true` memuat kunjungan itu, `awaitingClosureReason` = *"Masih ada observasi yang belum diselesaikan."* | 13 |
| S14 | Lanjutan S13: `{ "observationStatus": 2, "notes": "membaik sesudah penanganan" }` pada observasi Dieskalasi | `200`; `completionSummary` tersimpan; kunjungan `Completed` pada permintaan yang sama, `closedByDispositionId` terisi (baca baris), pelaku = akun uji | 14 |
| S15 | Ulang S13 pada kunjungan lain, lalu `{ "observationStatus": 4 }` | `200`; kunjungan `Completed` | 15 |
| S16 | Ulang S13 pada kunjungan lain, lalu `PATCH /emergency-visits/{id}/complete` | `409` *"Masih ada observasi yang belum diselesaikan."*; kunjungan tetap `Disposed` | 16 |
| S17 | Kunjungan uji S6: `{ "observationStatus": 2 }`, lalu `{ "observationStatus": 3 }`, lalu `{ "observationStatus": 2, "notes": "<1.200 karakter>" }`, terakhir `{ "observationStatus": 4 }` | Tiga permintaan pertama `409` dengan kalimat aturan 18 persis, observasi tetap `Escalated`; permintaan keempat `200`, kunjungan tetap `Completed` | 17 |
| S18 | Kunjungan `Completed` dengan periode `Escalated` (jalankan **sebelum** langkah keempat S17, atau pakai kunjungan lain): `POST /emergency-observation-details` pada periode itu. Pembanding: `POST` pada periode `Active` milik kunjungan yang masih berjalan; `PUT` pada baris pemantauan lama | `409` dengan kalimat aturan 21 persis, nol baris baru; pembanding `201`; `PUT` berperilaku seperti sebelumnya | 18 |
| S19 | Ulang S1, S5, S7–S11 laporan ini | Hasil sama dengan bagian 5.1 | 19 |

### Acceptance criteria — perluasan

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 13 | Eskalasi lalu disposisi dijalankan → kunjungan tetap `Disposed`, tampil menunggu penutupan dengan alasan observasi | **Terpenuhi pada source** | `ValidateVisitClosureAsync` menghitung `Escalated`; penutupan susulan `BE-IGD-060` memakai penjaga itu. Uji S13 belum |
| 14 | Observasi `Escalated` diselesaikan → `200`, kunjungan `Completed`, `ClosedByDispositionId` terisi | **Terpenuhi pada source** | Jalur `IGD-DEC-171` (tanpa perpindahan status pada `Disposed`) + penutupan susulan yang sudah ada. Uji S14 belum |
| 15 | Observasi `Escalated` dibatalkan → kunjungan tertutup | **Terpenuhi pada source** | `Cancelled` pada `Disposed` sudah memicu penutupan; penjaga kini lolos. Uji S15 belum |
| 16 | `complete` manual ditolak `409` | **Terpenuhi pada source** | `EmergencyVisitController` memakai penjaga yang sama. Uji S16 belum |
| 17 | Aksi observasi pada kunjungan berakhir → `409` aturan 18; `Cancelled` lolos | **Terpenuhi pada source** | Penolakan di `UpdateObservationStatus` sebelum eskalasi dan batas catatan. Uji S17 belum |
| 18 | Pemantauan baru pada kunjungan berakhir → `409` aturan 21; `PUT` tidak berubah | **Terpenuhi pada source** | Langkah 2a di `ValidateDetailScopeAsync`, hanya untuk `tolakPeriodeTertutup`. Uji S18 belum |
| 19 | Regresi acceptance 1, 5, 8–12 | **Belum diuji** | Uji S19 |
| 7 | Build 0 error | **Terpenuhi — 4 Oktober 2026** | Build Rizki 3 Oktober 2026: 0 error, 0 warning |

### Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kunjungan yang saat ini sudah `Disposed` menunggu penutupan karena penahan lain dan kebetulan punya observasi `Escalated` akan ikut tertahan oleh observasi itu begitu perubahan ini aktif (validation §11.2 aturan 20) |
| Masalah yang diketahui | S2, S3, dan S12 tetap tertahan `403` kewenangan unit (`BE-IGD-039`) — tidak berubah oleh pengerjaan ulang ini. Tombol tambah pemantauan di layar tidak dinonaktifkan pada kunjungan berakhir (di luar `IGD-DEC-187`) |
| Risiko tersisa | Rendah — perubahan berupa satu kondisi penjaga dan dua penolakan yang didahulukan; nol tulisan baru, nol schema |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Tiga berkas source IGD di atas, ditambah dokumen blueprint 3 Oktober 2026 (`docs/module-blueprints/igd/**`). Tanpa stage, commit, atau push |
| Langkah berikutnya | Rizki: `dotnet build`, lalu uji API S13–S19. Agent: pengerjaan ulang `FE-IGD-042` (`IGD-DEC-187`), dengan uji layar dalam satu siklus |

Putusan pengerjaan ulang: **🟡 sebagian** — acceptance 13–18 terpenuhi pada source dan QBE Strict `PASS`; build
(kriteria 7) dan uji API S13–S19 belum. Skenario S2, S3, dan S12 lama (acceptance 2 dan 3) tetap tertahan `BE-IGD-039`.

### Build pemilik — dicatat 4 Oktober 2026

| Butir | Isi |
| --- | --- |
| Perintah | `dotnet build` oleh Rizki, 3 Oktober 2026 |
| Hasil | **0 error, 0 warning** — dilaporkan pemilik 4 Oktober 2026 |
| Artefak | `bin/Debug/net9.0/QuilvianSystemBackend.dll` 3 Oktober 2026 18.55. Edit source terakhir pengerjaan ulang 18.34 (`EmergencyObservationService.cs`), jadi build memuat ketiga berkas |
| Commit | Source dan dokumen blueprint 3 Oktober di-commit pemilik sebagai `c1f79f79` (4 Oktober 2026 11.32, `rizkiG`, `ahead 12`) |
| Kriteria 7 | **Terpenuhi** |

Putusan: **🟡 sebagian** — sisa uji API S13–S19. Status baru dapat naik sesudah bukti mentah uji itu diperiksa; S2,
S3, dan S12 tetap tertahan `BE-IGD-039`.

Uji S13–S19 dijalankan lewat [panduan uji gabungan `MVP-8`](../../../testing/2026-10-04-panduan-uji-gabungan-mvp-8.md)
(4 Oktober 2026), satu siklus dengan uji layar `FE-IGD-041` dan `FE-IGD-042`. **Koreksi pada skenario S18:** pembanding
pemantauan yang berhasil dijawab **`200`**, bukan `201` — `EmergencyObservationDetailController.Create` mengembalikan
`Ok(...)`. Kunjungan uji S6 dicari ulang lewat kueri `SELECT` karena bukti mentah 2 Oktober tidak lagi ada di
repository frontend.

---

## Pemeriksaan bukti uji gabungan `MVP-8` — 4 Oktober 2026

Bukti mentah di `QuilvianSystemFrontendDev/test-with-agy/igd/uji-gabungan-mvp-8-2026-10-04/` (JSON per skenario, skrip
`runner-block-*.mjs`, `P7.json`, `data-uji.json`), dicocokkan dengan log backend `Logs/quilvian-backend-20261004.json`.
[Laporan penguji](../../../testing/2026-10-04-laporan-uji-gabungan-mvp-8.md) **tidak** dipakai sebagai bukti: ringkasannya
tidak cocok dengan bukti mentah pada beberapa butir (laporan penguji bagian 7).

**Keabsahan putaran.** Agen penguji mengubah Akses Role *Perawat IGD* lewat SuperAdmin sebelum uji dimulai (13.04.19),
melanggar aturan A3 dan A4 panduan. Pemilik mengesahkan izin itu dan menerima bukti dengan penyimpangan tercatat
(`IGD-DEC-188`). Log backend selama jendela uji (13.15–14.20): seluruh penulisan klinis IGD — 10 kunjungan, 10
observasi, 21 perubahan status observasi, 7 tindak lanjut, 2 pemantauan, 1 kepergian — oleh akun Perawat IGD; 10
encounter oleh akun Petugas Pendaftaran lewat `POST /patient-encounters/admin`; SuperAdmin hanya 3 `GET`.

| Skenario | Putusan | Yang teramati pada bukti mentah |
| --- | --- | --- |
| `061-S13` | **Terbukti** | `K1`: eskalasi `200`, tindak lanjut `Executed`; `GET /emergency-visits/{K1}` `visitStatus` 7; daftar `awaitingClosure=true&search=<K1>` tepat satu baris, `awaitingClosureReason` *"Masih ada observasi yang belum diselesaikan."* |
| `061-S14` | **Terbukti** | `PATCH` `{ 2, "membaik sesudah penanganan" }` `200`, `completionSummary` tersimpan; kunjungan 9; baris: `ClosedByDispositionId` = tindak lanjut `K1`, `UpdateBy` = akun perawat, `EncounterStatus` 9 |
| `061-S15` | **Terbukti** | `K2`: `{ 4 }` `200`; kunjungan 9; `ClosedByDispositionId` = tindak lanjut `K2` |
| `061-S16` | **Terbukti** | `K3`: `PATCH …/complete` `409` *"Masih ada observasi yang belum diselesaikan."*; kunjungan tetap 7, observasi tetap 3 |
| `061-S17` | **Terbukti** | `D-S6` = `IGD-261002015800-AD264E` (kunjungan 9, observasi 3). Langkah 1–3 masing-masing `409` dengan kalimat aturan 18 persis — langkah 3 (catatan 1.200 karakter) bukan `400`; observasi tetap 3, alasan eskalasi utuh. Langkah 4 `{ 4 }` `200`; kunjungan tetap 9, `ClosedByDispositionId` tetap tindak lanjut 2 Oktober. Urutan mengikat dipatuhi: langkah 1–3 06.28.02–03, S18 06.28.04–06, `042-U12` 06.28.06–24, langkah 4 06.28.24 UTC |
| `061-S18` | **Terbukti** (kaki `PUT` `NOT RUN`) | (a) `POST` pada `D-S6` `409` *"Kunjungan IGD ini sudah berakhir; pemantauan observasi tidak dapat ditambahkan lagi."*; jumlah baris 0 → 0. (b) Pembanding `K4` `200`, jumlah 0 → 1. (c) Periode `D-S6` tanpa baris pemantauan — sah sebagai `NOT RUN` menurut panduan |
| `061-S19-S7` | **Terbukti** | `K6` `Disposed`: eskalasi `409` *"Tindak lanjut pasien sudah dilaksanakan; eskalasi tidak dapat dicatat pada kunjungan ini."*; observasi tetap 1, `escalationReason` kosong, kunjungan 7 |
| `061-S19-S8` | **Terbukti** | Catatan 1.234 karakter: `409` kalimat yang sama, bukan `400` |
| `061-S19-S9` | **Terbukti** | Selesaikan dengan catatan 1.234 karakter: `400` *"Catatan paling banyak 1000 karakter."*; observasi 1, kunjungan 7 |
| `061-S19-S1` | **Terbukti** | `{ 2, "tanda vital stabil" }` `200`; kunjungan 9; `ClosedByDispositionId` = tindak lanjut `K6`, `UpdateBy` = perawat, `EncounterStatus` 9 |
| `061-S19-S5` | **Terbukti** | `K7` (observasi + kepergian): observasi `200`; kunjungan tetap 7, alasan *"Masih ada proses kepergian pasien yang belum selesai."* |
| `061-S19-S11` | **Terbukti** kaki `cancel`; kaki `reject-handover` `NOT RUN` | `PATCH …/cancel` `200`; kunjungan `K7` 9. Kaki `reject-handover` opsional pada panduan dan tidak dijalankan |
| `061-S19-S10` | **Terbukti** | `K8` (5): selesaikan `200` → kunjungan 6. `K9` (4): eskalasi `200` → kunjungan 4 — tanpa kalimat aturan 14 maupun 18 |

**Catatan.**

- **Penyimpangan resep data, tidak dilaporkan penguji, tidak memengaruhi putusan.** Sebelum membuat tindak lanjut, skrip
  menyisipkan `PATCH /emergency-visits/{id}/visit-status` `{ 6 }`: tindak lanjut hanya dapat dijalankan dari *Menunggu
  keputusan*. Resep `RD`/`RX` pada panduan kurang langkah itu — kekeliruan panduan, bukan task ini.
- **Jenis tindak lanjut di dev berkode `PLG`** (*Pulang / Rawat Jalan*), bukan `PULANG` seperti seeder — master data dev
  berbeda dari seeder; dicatat saja.
- **`IGD-OQ-112` (dev).** Kueri P7 sebelum S17: **4** observasi `Escalated` tertinggal pada kunjungan `Completed` —
  `IGD-261002015800-AD264E`, `IGD-261001084730-A609F2`, `IGD-261001084619-318470`, `IGD-261001084131-4037AB`. Sesudah S17
  langkah 4 tersisa **3**. Lingkungan lain belum dihitung.
- Acceptance 18 bagian *"`PUT` tidak berubah"* hanya terbukti lewat source: langkah 2a berada di balik
  `tolakPeriodeTertutup`, yang bernilai `false` pada `PUT`.

| # | Kriteria | Status |
| ---: | --- | --- |
| 13 | Eskalasi lalu disposisi dijalankan → tetap `Disposed`, tampil menunggu penutupan dengan alasan observasi | **Terpenuhi — S13** |
| 14 | Observasi `Escalated` diselesaikan → `200`, kunjungan `Completed`, `ClosedByDispositionId` terisi | **Terpenuhi — S14** (juga menutup kriteria 8) |
| 15 | Observasi `Escalated` dibatalkan → kunjungan tertutup | **Terpenuhi — S15** |
| 16 | `complete` manual ditolak `409` | **Terpenuhi — S16** |
| 17 | Aksi observasi pada kunjungan berakhir → `409` aturan 18; `Cancelled` lolos | **Terpenuhi — S17** |
| 18 | Pemantauan baru pada kunjungan berakhir → `409` aturan 21; `PUT` tidak berubah | **Terpenuhi** untuk `POST` (S18); `PUT` lewat source |
| 19 | Regresi acceptance 1, 5, 8–12 | **Terpenuhi — S19** |

Putusan: **🟡 sebagian** — acceptance 1, 5–19 terpenuhi pada bukti; acceptance 4 sebagian (kaki observasi saja, 2
Oktober). **Belum:** acceptance 2 dan 3 — pemicu `accept-handover`/`reject-handover` dan sikap pesanan tertahan `403`
kewenangan unit (`BE-IGD-039`), sesuai `IGD-DEC-185`.
