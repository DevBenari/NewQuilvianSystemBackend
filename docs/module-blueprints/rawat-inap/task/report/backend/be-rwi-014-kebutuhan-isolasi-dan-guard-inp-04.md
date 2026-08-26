# Laporan Perubahan Backend — `BE-RWI-014`

> **Pembaruan 26 Agustus 2026 — validasi sudah dijalankan.** Field `Status` di bawah beserta
> setiap baris `NOT RUN` untuk `dotnet build` dan `dotnet test` pada laporan ini **sudah tidak
> berlaku**. Kedua perintah dijalankan 26 Agustus 2026 atas seluruh solution: **build 0 error**,
> dan **255 test hijau, 0 gagal**. Perinciannya ada pada
> [laporan validasi](be-rwi-validasi-build-dan-test.md).
>
> Yang **belum** berubah: acceptance criteria dan DoD task ini tetap belum terbukti penuh —
> build hijau bukan tanda selesai — sehingga tandanya pada roadmap tetap 🟡.

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-014` |
| Judul | Kebutuhan isolasi tercatat pada episode dengan pemiliknya jelas |
| Slice | S2 — Pasien punya lokasi, dan penempatan yang tidak layak ditolak |
| Trace | `RWI-DEC-065`; `RWI-RULE-012` bagian A aturan 1–4; `GUARD-INP-04`; api contract `PATCH /episodes/{id}/isolation-requirement`; validation matrix bagian 4A; `FR-RI-158`, `FR-RI-159`; `RWI-AC-136`, `RWI-AC-137`, `RWI-AC-139`; `UAT-32` |
| Contract version | API `0.4.0` — bentuk tidak berubah; satu endpoint "Rencana" kini ada |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `bd97e5d` |
| Tanggal pengerjaan | 25 Agustus 2026 |
| Dependency | `BE-RWI-011` — dikerjakan pada sesi yang sama |
| Status | **IMPLEMENTASI SELESAI — VALIDASI BELUM DIJALANKAN** |

---

## 1. Apa yang dibangun

`PATCH /episodes/{id}/isolation-requirement` beserta `GUARD-INP-04`, dan penetapan
`InpIsolationSource` oleh sistem.

Sebelum task ini, sistem **tidak punya tempat sama sekali** untuk mencatat bahwa seorang pasien
membutuhkan isolasi. Keenam kolomnya sudah dibuat `BE-RWI-003`, tetapi tidak ada satu pun jalur
yang mengisinya.

---

## 2. `GUARD-INP-04` — inti task ini

Mesin hak akses repository ini hanya mengenal **"peran ini boleh memanggil endpoint ini"**. Ia
menjawab `SetIsolation` dengan "boleh" untuk petugas admisi **dan** untuk dokter mana pun.

Yang membedakan keduanya adalah **status episode** dan **siapa DPJP aktifnya**:

| Keadaan | Pelaku | Hasilnya |
| --- | --- | --- |
| `Draft` | Petugas admisi (bukan dokter) | Diterima; `IsolationSource = AdmissionRecord`, `IsolationSetByDoctorId` **kosong** |
| `Draft` | DPJP aktif | Diterima; `IsolationSource = ClinicalDecision` |
| `Draft` | Dokter yang bukan DPJP aktif | **Ditolak 403** |
| `Admitted` / `DischargePending` | DPJP aktif | Diterima; `IsolationSource = ClinicalDecision` |
| `Admitted` / `DischargePending` | Petugas admisi | **Ditolak 403** — wewenangnya berhenti di `Draft` |
| `Admitted` / `DischargePending` | Dokter yang bukan DPJP aktif | **Ditolak 403** |
| `Closed` / `Cancelled` | Siapa pun | **Ditolak 409** |

> **Bila penjaga ini dilupakan.** Dokter jaga mana pun dapat mengubah keputusan pengendalian
> infeksi milik DPJP lain, dan tidak ada satu pun kolom yang dapat membedakannya dari keputusan
> yang sah. Kolom `IsolationSetByDoctorId` justru akan mencatat nama dokter jaga itu sebagai
> pengambil keputusan klinis.

### 2.1 Sumber catatan tidak pernah dikirim pemanggil

`SetIsolationRequirementRequest` **tidak punya** kolom `IsolationSource`. Sistem yang
menentukannya, sesuai validation matrix bagian 4A baris ketiga.

Menerimanya dari pemanggil akan membuat catatan awal petugas admisi dapat menyamar sebagai
keputusan klinis DPJP — dan pembedaan antara keduanya adalah satu-satunya alasan `RWI-DEC-065`
membuat kolom `IsolationSource` sejak awal.

### 2.2 Perubahan tidak pernah ditahan penempatan

Pasien yang sedang berada di tempat tidur biasa **tetap boleh** dinyatakan membutuhkan isolasi.
Yang muncul adalah daftar pantau, bukan penolakan — dikerjakan `BE-RWI-015`.

> Menahan pencatatan klinis demi menjaga aturan penempatan adalah urutan yang terbalik: fakta
> klinis dicatat lebih dulu, lalu sistem menunjukkan penempatannya perlu dibetulkan.
> `RWI-RULE-012` bagian A aturan 7.

---

## 3. Yang berubah pada source

| Berkas | Jenis | Isi perubahan |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.Assignments.cs` | Ditambah | `SetIsolationRequirementAsync` beserta `GUARD-INP-04`; `IsActiveDoctorAsync`; `GetActiveDoctorIdAsync` |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientEpisodeAssignmentDtos.cs` | **Baru** | `SetIsolationRequirementRequest` |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientEpisodeController.cs` | Ditambah | Aksi `PATCH /{id}/isolation-requirement` dengan butir `InpatientEpisode : SetIsolation` |
| `Areas/HealthServices/InPatientManagement/Helpers/InpatientActorClaims.cs` | **Baru** | Pembacaan `user_id` dan `doctor_id` dari klaim, dipakai seluruh controller Rawat Inap |

### 3.1 Kenapa identitas dokter dibaca dari klaim

`GUARD-INP-04` — dan tiga penjaga lainnya — bergantung pada jawaban "apakah pengguna ini
seorang dokter, dan dokter yang mana". Jawaban itu dibaca dari klaim `doctor_id` yang
diterbitkan `AuthController`, **tidak pernah** dari isian permintaan.

Bila ia dibaca dari isian permintaan, seluruh penjaga dapat dilewati hanya dengan mengirim
identifier dokter lain.

Pembacaannya dikumpulkan di satu tempat karena kesalahan membaca nama klaim tidak menghasilkan
galat apa pun — ia hanya terlihat sebagai 403 yang membingungkan bagi DPJP yang sesungguhnya
berwenang.

---

## 4. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `InPatientManagement` |
| Pemilik/prefix pada registry | `InPatientManagement / Inpatient`, prefix `Inp` |
| Lifecycle registry | `ACTIVE` sejak 2026-08-24 (`RWI-DEC-068`) |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-AUD-001` |
| Pengecualian QBE | Tidak ada |

**`QBE-LOG-001` beserta aturan kolom sensitif.** Payload logger untuk endpoint ini **tidak**
memuat `IsolationNote`. Kolom itu bertanda sensitif pada permission matrix bagian 5.4 karena ia
memuat alasan klinis kebutuhan isolasi seorang pasien.

---

## 5. Keputusan implementasi yang perlu ditinjau

### 5.1 "Petugas admisi" diturunkan dari ketiadaan klaim `doctor_id`

Service membedakan dua jalur dari satu pertanyaan: apakah pengguna punya `doctor_id`. Yang
punya masuk jalur dokter dan wajib menjadi DPJP aktif; yang tidak punya masuk jalur petugas
admisi dan hanya boleh selagi `Draft`.

Konsekuensinya: **peran non-klinis apa pun** yang diberi butir `SetIsolation` akan diperlakukan
sebagai petugas admisi. Pembatasan sesungguhnya ada pada pemberian butir hak akses itu di layar
Role Access — dan pemetaan peran ke butir hak akses pada permission matrix bagian 3 masih
berstatus **usulan**, karena pemilik keamanan belum ditunjuk.

### 5.2 Kriteria 6 tidak dapat dibuktikan tanpa aplikasi berjalan

Acceptance criteria 6 — "peran di luar admisi dan dokter ditolak 403 oleh mesin hak akses,
sebelum service dijalankan" — memerlukan `AccessPermissionFilter` yang baru berjalan pada
permintaan HTTP sungguhan. Yang dapat dijaga sekarang adalah bahwa endpoint-nya memang memakai
butir `SetIsolation`, dan itu dijaga test kontrak.

---

## 6. Validasi

### 6.1 Yang **belum** dijalankan

| Perintah | Keadaannya |
| --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | ✅ **PASS** — Build succeeded, 0 Error(s); dijalankan 26 Agustus 2026, dan diulang tiga kali berturut-turut dengan hasil sama |
| `dotnet test QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` | ✅ **PASS** — Passed! Failed 0, Passed 255, Skipped 0, Total 255 |
| `UAT-32` terhadap aplikasi berjalan | **NOT RUN** |

### 6.2 Test yang ditulis

`QuilvianSystemBackend.Tests/InPatientManagement/InpIsolationRequirementTests.cs` — 6 test.

| Acceptance criteria | Test yang membuktikan | Status |
| --- | --- | --- |
| 1. Petugas admisi selagi `Draft` → `AdmissionRecord`, dokter kosong | `Kriteria1_PetugasAdmisiMenyalakanSelagiDraftMenghasilkanCatatanAwal` | ✅ **Lulus** 26 Agu 2026 |
| 2. DPJP aktif setelah `Admitted` → `ClinicalDecision`, dokter terisi | `Kriteria2_DpjpAktifMengubahSetelahAdmittedMenghasilkanKeputusanKlinis` | ✅ **Lulus** 26 Agu 2026 |
| 3. Dokter yang bukan DPJP aktif ditolak 403 | `Kriteria3Dan4_SetelahAdmittedHanyaDpjpAktifYangBoleh` | ✅ **Lulus** 26 Agu 2026 |
| 4. Petugas admisi setelah `Admitted` ditolak 403 | Test yang sama | ✅ **Lulus** 26 Agu 2026 |
| 5. Menyalakan tanpa keterangan ditolak 400 | `Kriteria5_MenyalakanTanpaKeteranganDitolak400` | ✅ **Lulus** 26 Agu 2026 |
| 6. Peran di luar admisi dan dokter ditolak mesin hak akses | **Tidak dapat diuji tanpa aplikasi berjalan** — bagian 5.2 | Tertunda |

Kriteria 3 dan 4 sengaja **berpasangan dalam satu test**, sesuai permintaan roadmap, supaya
terlihat bahwa yang membedakan diterima dan ditolak adalah status episode beserta hubungan
dokter dengan pasien itu — bukan sekadar peran penggunanya.

Dua test tambahan menjaga: mencabut kebutuhan isolasi tidak mewajibkan keterangan, dan episode
yang sudah dibatalkan tidak dapat diubah kebutuhan isolasinya.

---

## 7. Dampak

| Aspek | Dampaknya |
| --- | --- |
| API contract | **Aditif.** Satu endpoint baru dengan butir hak akses baru `InpatientEpisode : SetIsolation` |
| Database | Tidak ada perubahan schema; enam kolom yang diisi sudah dibuat `BE-RWI-003` |
| Keamanan | Penjaga kewenangan per pasien keempat ditambahkan. Ia hanya bekerja bila dipanggil — `RWI-RISK-004` |

---

## 8. Risiko yang tersisa

| Risiko | Akibatnya bila terwujud | Yang menutupnya |
| --- | --- | --- |
| **Build dan test belum dijalankan** | Keenam kriteria belum terbukti | Bagian 6.1 |
| Kriteria 6 belum terbukti | Peran di luar admisi dan dokter mungkin dapat mengubah kebutuhan isolasi | Verifikasi runtime |
| Pemetaan peran ke butir hak akses masih usulan | Butir `SetIsolation` dapat diberikan kepada peran yang tidak dimaksud | Penunjukan pemilik keamanan/privasi |
| `GUARD-INP-04` hanya bekerja bila dipanggil | Endpoint baru yang lupa memanggilnya lolos tanpa peringatan apa pun | Test yang diwajibkan `RWI-DEC-051`; ditegakkan lewat review |

---

## 9. Definition of Done

| Butir DoD | Keadaannya |
| --- | --- |
| Endpoint dan penjaga aktif | ✅ Ada di dalam kode |
| Keenam kriteria lulus | ❌ **Belum.** Kriteria 6 belum dapat diuji |
| Permission matrix dan kenyataan cocok | ✅ Butir `InpatientEpisode : SetIsolation` dipakai apa adanya |
| Api contract diperbarui | ❌ **Belum, dan memang belum boleh** |

---

## 10. Langkah berikutnya

1. Jalankan `dotnet build` dan `dotnet test`.
2. Verifikasi kriteria 6 lewat pemanggilan endpoint dengan peran yang tidak berhak.
3. Bawa pemetaan peran pada permission matrix bagian 3 ke pemilik keamanan setelah ditunjuk.
