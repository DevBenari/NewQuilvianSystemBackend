# Laporan Perubahan Backend — `BE-RWI-025`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-025` |
| Judul | Kelima syarat penutupan diperiksa dan dilaporkan satu per satu |
| Slice | S6 — Episode dapat ditutup dan tempat tidur kembali kosong |
| Trace | `RWI-DEC-016`; `RWI-RULE-010`; api contract `GET .../closure-readiness` dan `POST .../close`; `UAT-11`; `RWI-AC-064` |
| Contract version | API `0.4.0` — bentuk tidak berubah; dua endpoint "Rencana" kini ada |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `bd97e5d` |
| Tanggal pengerjaan | 25 Agustus 2026 |
| Dependency | `BE-RWI-022`, `BE-RWI-023`, `BE-RWI-024` — ketiganya dikerjakan pada sesi yang sama |
| Status | **IMPLEMENTASI SELESAI — VALIDASI BELUM DIJALANKAN** |

---

## 1. Kelima syarat, dan bentuknya adalah kontrak

| No | Kode | Syarat | Dapat ditembus supervisor |
| ---: | --- | --- | :---: |
| 1 | `DISCHARGE_DECIDED` | Keputusan pulang dari DPJP sudah ada | Tidak |
| 2 | `SUMMARY_SIGNED` | Resume pulang sudah ditandatangani DPJP | Tidak |
| 3 | `CLEARANCE_COMPLETE` | Seluruh butir wajib administrasi sudah ditandai | Tidak |
| 4 | `FINANCIAL_CLEARED` | Kelayakan keuangan dinyatakan lunas kasir | **Ya** |
| 5 | `BED_STATE_RESOLVED` | Keadaan tempat tidur pasien sudah jelas | Tidak |

`GET /closure-readiness` mengembalikan **kelimanya** beserta tanda sudah atau belum, dan
kalimat yang dibaca petugas untuk setiap syarat yang belum terpenuhi.

> **Kriteria 1 sering dikerjakan sebagai boolean karena lebih sederhana.** Layar kemudian
> hanya dapat mematikan tombol tutup tanpa dapat memberi tahu petugas apa yang harus dikejar —
> dan petugas menebak, lalu menghubungi orang yang salah. Bentuk daftar dikunci api contract
> dan `RWI-RULE-010`; ia bukan preferensi.

---

## 2. Syarat kelima perlu penjelasan

`RWI-RULE-010` menuliskan syarat kelima sebagai *"tempat tidur aktif ditemukan, sesuai INV-02
dan `RWI-RULE-008`"*.

Sejak `RWI-DEC-055` melonggarkan `INV-INP-01`, episode `DischargePending` yang kepergiannya
sudah dicatat memang **tidak lagi** memegang tempat tidur — dan episode itu tetap harus dapat
ditutup (`BE-RWI-027` kriteria 4).

Syarat kelima karena itu dibaca sebagai: **episode memegang penempatan aktif yang akan dilepas
saat penutupan, atau kepergiannya sudah dicatat.** Yang gagal hanyalah episode yang tidak
punya keduanya — keadaan yang seharusnya mustahil, dan justru karena itu perlu terlihat bila
terjadi.

**Ini interpretasi, bukan penulisan ulang aturan.** Bila Product/Domain membacanya berbeda,
sebutkan; perubahannya satu baris.

---

## 3. Apa yang terjadi saat episode ditutup

Seluruhnya di dalam **satu transaksi**:

1. penempatan aktif ditutup dengan alasan `EpisodeClosed`;
2. salinan `MstBed.BedStatus` kembali `Available`;
3. penugasan DPJP dan perawat yang masih aktif ditutup;
4. `ClosedAt` terisi;
5. status menjadi `Closed` lewat `ApplyStatusChangeAsync`, yang menulis satu baris riwayat.

### 3.1 Kenapa penugasan ikut ditutup

Tanpa langkah ketiga, riwayat penugasan berakhir menggantung: DPJP terlihat masih bertanggung
jawab atas pasien yang sudah pulang berbulan-bulan lalu, dan pertanyaan "siapa DPJP pada
tanggal tertentu" menjawab benar untuk tanggal yang salah.

---

## 4. Yang berubah pada source

| Berkas | Jenis | Isi perubahan |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs` | Ditambah | `EvaluateClosureReadinessAsync`, `CloseEpisodeAsync`, `BuildClosureConditionsAsync`, `CloseEpisodeInternalAsync`, `CloseActiveAssignmentsAsync` |
| `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.cs` | Diubah | Class menjadi `partial`; konstruktor menerima `InpBedOccupancyService` |
| `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs` | Ditambah | `ReleaseActivePlacementAsync` yang ikut transaksi pemanggilnya |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientClosureDtos.cs` | Ditambah | `ClosureConditionResponse`, `ClosureReadinessResponse`, `CloseEpisodeRequest` |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeController.cs` | Ditambah | Dua aksi penutupan |

### 4.1 Delta terhadap class diagram blueprint

`InpDischargeService` kini juga memakai `InpBedOccupancyService`, karena pelepasan tempat tidur
milik service itu. **Arahnya tidak melingkar**: `InpBedOccupancyService` memakai
`InpEpisodeService`, dan `InpDischargeService` memakai keduanya — tidak ada satu pun yang
menunjuk balik.

Class diagram `02-backend-architecture.md` bagian 3.4 hanya menggambar panah
`InpDischargeService --> InpEpisodeService`. Diagramnya perlu ditambah satu panah. **Owner:
pemilik arsitektur backend.** Ini delta kedua terhadap diagram yang sama; yang pertama dicatat
pada laporan `BE-RWI-011` bagian 3.1.

### 4.2 `ReleaseActivePlacementAsync` tidak membuka transaksi sendiri

Ia ikut transaksi pemanggilnya, dan pemanggilnya wajib memanggil `SaveChangesAsync`.

> **Kenapa.** Melepas tempat tidur di dalam transaksi terpisah membuka keadaan setengah jadi
> yang paling merugikan: tempat tidur sudah bebas dan diambil pasien lain, sementara tindakan
> yang menyebabkannya gagal dan pasien lama masih tercatat di sana.

---

## 5. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `InPatientManagement` |
| Pemilik/prefix pada registry | `InPatientManagement / Inpatient`, prefix `Inp` |
| Lifecycle registry | `ACTIVE` sejak 2026-08-24 (`RWI-DEC-068`) |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-DEL-001`, `QBE-AUD-001` |
| Pengecualian QBE | Tidak ada |

**`QBE-TXN-001`** adalah aturan penentu: kelima perubahan pada bagian 3 berada di dalam satu
transaksi.

---

## 6. Validasi

| Perintah | Keadaannya |
| --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | **NOT RUN** |
| `dotnet test QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` | **NOT RUN** |
| `UAT-11` terhadap aplikasi berjalan | **NOT RUN** |

### 6.1 Test yang ditulis

`QuilvianSystemBackend.Tests/InPatientManagement/InpEpisodeClosureTests.cs`.

| Acceptance criteria | Test yang membuktikan | Status |
| --- | --- | --- |
| 1. `closure-readiness` mengembalikan kelima syarat, bukan boolean | `Kriteria1_ClosureReadinessMengembalikanKelimaSyaratBesertaTandanya` | Ditulis, **belum dijalankan** |
| 2. Penutupan dengan syarat kurang ditolak 422 disertai daftarnya | `Kriteria2_PenutupanDenganSyaratBelumTerpenuhiDitolak422DisertaiDaftarnya` | Ditulis, **belum dijalankan** |
| 3. Penutupan mengubah episode menjadi `Closed` dan melepas tempat tidur dalam satu transaksi | `Kriteria3Dan4Dan5_PenutupanMelepasTempatTidurDanMenulisSatuBarisRiwayat` | Ditulis, **belum dijalankan** |
| 4. Tempat tidur terbaca `Available` pada pencarian berikutnya | Test yang sama | Ditulis, **belum dijalankan** |
| 5. Penutupan menulis satu baris riwayat status | Test yang sama | Ditulis, **belum dijalankan** |

Verifikasi yang diminta roadmap — "test yang menutup episode lalu mencari tempat tidur kosong
dan menemukannya" — dijawab test kriteria 3: `SearchAvailableBedsAsync` dipanggil sebelum dan
sesudah penutupan, dan hasilnya berubah dari kosong menjadi satu tempat tidur.

Satu test tambahan menjaga bahwa episode yang sudah ditutup tidak dapat ditutup lagi.

---

## 7. Dampak

| Aspek | Dampaknya |
| --- | --- |
| API contract | **Aditif.** Dua endpoint baru |
| Database | Tidak ada perubahan schema |
| Arsitektur | Satu panah tambahan pada class diagram — bagian 4.1 |
| Modul tetangga | Salinan `MstBed.BedStatus` kembali `Available` saat episode ditutup |

---

## 8. Risiko yang tersisa

| Risiko | Akibatnya bila terwujud | Yang menutupnya |
| --- | --- | --- |
| **Build dan test belum dijalankan** | Kelima kriteria belum terbukti | Bagian 6 |
| Interpretasi syarat kelima belum dikonfirmasi | Episode tanpa penempatan dan tanpa catatan kepergian tertahan, atau justru lolos | Konfirmasi Product/Domain — bagian 2 |
| Class diagram blueprint berselisih dengan source | Pembaca berikutnya menulis kode mengikuti diagram | Koreksi `02-backend-architecture.md` bagian 3.4 |

---

## 9. Definition of Done

| Butir DoD | Keadaannya |
| --- | --- |
| Dua endpoint sesuai kontrak | ✅ Ada di dalam kode |
| Kelima kriteria lulus | ❌ **Belum.** Test ditulis, belum dijalankan |
| Api contract diperbarui | ❌ **Belum, dan memang belum boleh** |

---

## 10. Langkah berikutnya

1. Jalankan `dotnet build` dan `dotnet test`.
2. Konfirmasi interpretasi syarat kelima (bagian 2).
3. Koreksi class diagram `02-backend-architecture.md` bagian 3.4 — dua panah sekaligus.
