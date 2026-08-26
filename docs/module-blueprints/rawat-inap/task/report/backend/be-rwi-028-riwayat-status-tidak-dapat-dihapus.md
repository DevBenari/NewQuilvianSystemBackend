# Laporan Perubahan Backend — `BE-RWI-028`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-028` |
| Judul | Riwayat status terbaca lengkap dan tidak dapat dihapus |
| Slice | S7 — Riwayat, daftar pantau, dan koreksi |
| Trace | `RWI-DEC-009`; `NFR-003`; api contract `GET /episodes/{id}/status-history`; `UAT-17` |
| Contract version | API `0.4.0` — bentuk tidak berubah; satu endpoint "Rencana" kini ada |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `bd97e5d` |
| Tanggal pengerjaan | 25 Agustus 2026 |
| Dependency | `BE-RWI-027` — dikerjakan pada sesi yang sama |
| Status | **IMPLEMENTASI SELESAI — VALIDASI BELUM DIJALANKAN** |

---

## 1. Apa yang dibangun

`GET /episodes/{id}/status-history`. Baris riwayatnya sudah ditulis sejak `BE-RWI-007` lewat
`ApplyStatusChangeAsync`; task ini menambahkan **pembacaannya** dan membuktikan sifat tidak
dapat diubahnya.

---

## 2. Kriteria 2 dijawab dengan ketiadaan, bukan dengan penolakan

Roadmap meminta verifikasinya dijalankan dengan "mencoba `PUT` dan `DELETE` langsung dan
membuktikan keduanya tidak tersedia".

**Bentuk penegakannya adalah ketiadaan endpoint.** Tidak ada satu pun rute pada modul Rawat
Inap yang mengubah maupun menghapus `InpStatusHistory`, dan tidak ada satu pun `DELETE` pada
kelima controllernya.

Ini disengaja dan tertulis: api contract bagian 8 baris pertama menyebutkannya sebagai
keputusan, dan `RWI-RULE-031` aturan 5 mendasarinya.

Dua test menjaganya:

| Test | Yang dijaga |
| --- | --- |
| `RiwayatStatusHanyaDapatDibaca` | Tepat satu rute menyentuh riwayat status, dan ia `GET` dengan butir `Read` |
| `TidakAdaSatuPunEndpointDeleteDiSeluruhModul` | Tidak ada `DELETE` pada keempat controller modul |

---

## 3. Kriteria 3 adalah masalah keadilan, bukan teknis

Perubahan yang **dihitung sistem** — episode `Draft` yang gugur sendiri — tercatat sebagai
tindakan sistem dengan kolom pelaku **kosong**.

> **Kejadian yang dicegah.** Sdri. Wati membuka daftar episode pukul 09:00. Pada saat itu juga,
> sistem menghitung bahwa satu episode `Draft` milik petugas lain sudah telantar 25 jam dan
> membatalkannya. Bila pembatalan itu dicatat atas nama Sdri. Wati, laporan pengecualian akan
> menunjukkan bahwa **ia** yang membatalkan admisi orang lain — dan ia tidak melakukan apa-apa
> selain membuka layar.

Perilaku ini sudah benar sejak `BE-RWI-008` menulis `ActorType = System` dan
`ChangedByUserId = null`. Task ini **membuktikannya terbaca** lewat endpoint, dan menjaganya
dengan test.

---

## 4. Yang berubah pada source

| Berkas | Jenis | Isi perubahan |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.Reads.cs` | Ditambah | `GetStatusHistoryAsync` |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientCorrectionDtos.cs` | **Baru** | `InpatientStatusHistoryResponse` |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientEpisodeController.cs` | Ditambah | Aksi `GET /{id}/status-history` |

Tidak ada penyaringan status pada pembacaannya — justru episode yang sudah `Closed` yang paling
sering ditelusuri auditor.

---

## 5. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `InPatientManagement` |
| Pemilik/prefix pada registry | `InPatientManagement / Inpatient`, prefix `Inp` |
| Lifecycle registry | `ACTIVE` sejak 2026-08-24 (`RWI-DEC-068`) |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001`, `QBE-DEL-001`, `QBE-AUD-001` |
| Pengecualian QBE | Tidak ada |

**`QBE-DEL-001`** relevan justru karena yang **tidak** disediakan. **`QBE-AUD-001`** — jejak
database ini terpisah dari `LoggerService`, dan hanya yang pertama yang menjadi bukti tindakan
bisnis.

---

## 6. Keputusan implementasi yang perlu ditinjau

### 6.1 Satu test lama perlu disesuaikan

`TidakAdaEndpointYangMenyetelStatusSecaraBebas` sebelumnya menolak **setiap** rute yang memuat
kata `status`. Rute `GET /{id}/status-history` memuatnya, sehingga test itu disesuaikan: ia
kini hanya memeriksa rute yang **menulis**, dan menolak rute yang berakhir dengan `/status`.

Yang dijaga tidak berubah — endpoint bergaya `PATCH /episodes/{id}/status` yang menerima nilai
bebas tetap ditolak. Yang berubah hanya cara memeriksanya, supaya pembacaan riwayat tidak ikut
terjaring.

---

## 7. Validasi

| Perintah | Keadaannya |
| --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | **NOT RUN** |
| `dotnet test QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` | **NOT RUN** |
| `UAT-17` terhadap aplikasi berjalan | **NOT RUN** |

### 7.1 Test yang ditulis

`QuilvianSystemBackend.Tests/InPatientManagement/InpStatusHistoryAndMonitoringTests.cs`,
ditambah dua test kontrak.

| Acceptance criteria | Test yang membuktikan | Status |
| --- | --- | --- |
| 1. Riwayat terbaca urut beserta pelaku, status asal, tujuan, dan alasan | `Kriteria1Dan4_RiwayatTerbacaUrutDanTetapTerbacaSetelahEpisodeDitutup` | Ditulis, **belum dijalankan** |
| 2. Tidak ada endpoint yang mengubah atau menghapus baris riwayat | `RiwayatStatusHanyaDapatDibaca`, `TidakAdaSatuPunEndpointDeleteDiSeluruhModul` | Ditulis, **belum dijalankan** |
| 3. Perubahan yang dihitung sistem tercatat sebagai tindakan sistem | `Kriteria3_KedaluwarsaDicatatSebagaiTindakanSistemDenganPelakuKosong` | Ditulis, **belum dijalankan** |
| 4. Riwayat tetap terbaca setelah episode `Closed` | Test kriteria 1 | Ditulis, **belum dijalankan** |

Test kriteria 1 menelusuri satu episode dari lahir sampai tutup — tiga baris riwayat, urut,
dengan status asal dan tujuan yang bersambung.

Test kriteria 3 membaca episode kedaluwarsa memakai identitas pengguna **yang berbeda** dari
pembuatnya, lalu memeriksa bahwa kolom pelaku tetap kosong.

---

## 8. Dampak

| Aspek | Dampaknya |
| --- | --- |
| API contract | **Aditif.** Satu endpoint baru |
| Database | Tidak ada perubahan schema |
| Test existing | Satu test kontrak disesuaikan — bagian 6.1 |

---

## 9. Risiko yang tersisa

| Risiko | Akibatnya bila terwujud | Yang menutupnya |
| --- | --- | --- |
| **Build dan test belum dijalankan** | Keempat kriteria belum terbukti | Bagian 7 |
| Sifat tidak dapat diubah ditegakkan lewat ketiadaan endpoint, bukan lewat database | Perubahan langsung di database tetap mungkin | Di luar scope modul; perlu kebijakan akses database |

---

## 10. Definition of Done

| Butir DoD | Keadaannya |
| --- | --- |
| Endpoint sesuai kontrak | ✅ Ada di dalam kode |
| Keempat kriteria lulus | ❌ **Belum.** Test ditulis, belum dijalankan |
| Api contract diperbarui | ❌ **Belum, dan memang belum boleh** |

---

## 11. Langkah berikutnya

1. Jalankan `dotnet build` dan `dotnet test`.
2. Jalankan `UAT-17` terhadap aplikasi berjalan.
