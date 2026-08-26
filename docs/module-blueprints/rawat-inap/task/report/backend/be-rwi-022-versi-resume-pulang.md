# Laporan Perubahan Backend — `BE-RWI-022`

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
| Task ID | `BE-RWI-022` |
| Judul | Koreksi resume menyimpan versi sebelumnya |
| Slice | S5 — Pasien dapat dinyatakan boleh pulang |
| Trace | `RWI-DEC-057`; `FR-RI-153`; `RWI-AC-124` s.d. `RWI-AC-126`; `UAT-27` |
| Contract version | API `0.4.0` — bentuk tidak berubah; parameter `includeRevisions` kini bekerja |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `bd97e5d` |
| Tanggal pengerjaan | 25 Agustus 2026 |
| Dependency | `BE-RWI-021` — dikerjakan pada sesi yang sama |
| Status | **IMPLEMENTASI SELESAI — VALIDASI BELUM DIJALANKAN** |

---

## 1. Apa yang dibangun

Penyalinan versi resume di dalam `InpDischargeService`, dan parameter `includeRevisions` pada
`GET /discharges/{episodeId}/summary`.

---

## 2. Dua jalur yang sangat berbeda, dan membedakannya adalah inti task ini

| Keadaan resume | Yang terjadi saat isinya diubah |
| --- | --- |
| **Belum** ditandatangani | Isinya ditimpa biasa. **Tidak ada versi yang disimpan** |
| **Sudah** ditandatangani | Amandemen rekam medis. Salinan versi sebelumnya disimpan lebih dulu, di dalam transaksi yang sama |

> **Kenapa kriteria 1 dan 2 mudah dikerjakan terbalik.** "Setiap penyuntingan membuat versi"
> terdengar lebih aman dan lebih mudah ditulis. Akibatnya: tabel versi terisi draf setengah jadi
> — DPJP yang mengetik diagnosis lalu membetulkan salah ketiknya melahirkan tiga versi dalam
> lima menit. Riwayat amandemen yang berisi tiga puluh baris draf kehilangan artinya sebagai
> riwayat amandemen, dan auditor tidak lagi dapat menemukan koreksi yang benar-benar berarti.

---

## 3. Apa yang tersimpan pada satu versi

| Kelompok | Kolom |
| --- | --- |
| Isi resume lama | `PrimaryDiagnosisText`, `SecondaryDiagnosisText`, `ProcedureSummary`, `DischargeMedicationNote`, `FollowUpInstruction`, `ReferralDestination`, `ClinicalSummary` |
| Keadaan lama | `PreviousDischargeType` |
| Tanda tangan lama | `PreviousSignedAt`, `PreviousSignedByDoctorId` |
| Penggantinya | `SupersededAt`, `SupersededByUserId` |
| Penyebabnya | `CorrectionSessionId` |

Nama penandatangan lama karena itu **tetap dapat dibaca selamanya**, walaupun resume yang
berlaku sekarang ditandatangani orang lain.

---

## 4. Versi tidak dapat diubah dan tidak dapat dihapus

Roadmap meminta verifikasinya dijalankan dengan "mencoba `PUT` dan `DELETE` langsung ke baris
versi dan membuktikan keduanya ditolak".

**Bentuk penolakannya adalah ketiadaan endpoint.** Tidak ada satu pun rute pada
`InpatientDischargeController` yang menunjuk baris versi, dan tidak ada satu pun `DELETE` pada
controller itu. Ini disengaja — api contract bagian 8 menyebutkannya sebagai keputusan, bukan
kelalaian.

Test kriteria 3 memeriksanya lewat refleksi atas rute controller: tidak ada template yang
memuat kata `revision`, dan tidak ada `HttpDelete` sama sekali.

---

## 5. Yang berubah pada source

| Berkas | Jenis | Isi perubahan |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.cs` | Ditambah | `AddRevisionSnapshotAsync`; cabang amandemen di dalam `UpsertSummaryAsync`; pembacaan versi pada `GetSummaryAsync` |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientDischargeDtos.cs` | Ditambah | `DischargeSummaryRevisionResponse`; kolom `Revisions` pada `DischargeSummaryResponse` |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeController.cs` | Ditambah | Parameter `includeRevisions` pada `GET .../summary` |

Tabel `InpDischargeSummaryRevision` sudah dibuat `BE-RWI-003`; tidak ada perubahan schema.

---

## 6. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `InPatientManagement` |
| Pemilik/prefix pada registry | `InPatientManagement / Inpatient`, prefix `Inp` |
| Lifecycle registry | `ACTIVE` sejak 2026-08-24 (`RWI-DEC-068`) |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-DEL-001`, `QBE-AUD-001` |
| Pengecualian QBE | Tidak ada |

**`QBE-DEL-001`** relevan justru karena yang **tidak** disediakan: tidak ada jalur penghapusan
maupun penyuntingan untuk baris versi, dan ketiadaannya adalah bagian dari kontrak.

**`QBE-TXN-001`** — penyalinan versi dan penulisan isi baru berada di dalam satu transaksi.
Bila salah satu gagal, resume lama tetap utuh dan tidak ada versi setengah jadi yang tersimpan.

---

## 7. Keputusan implementasi yang perlu ditinjau

### 7.1 Amandemen hanya dapat dilakukan supervisor

State matrix bagian 5 baris 5 menetapkan perubahan resume tertandatangani dilakukan
**supervisor**, di dalam sesi koreksi. Implementasi mengikutinya: DPJP aktif sekalipun ditolak
403 bila bukan supervisor.

Konsekuensinya perlu diketahui: DPJP yang menemukan kesalahan pada resumenya sendiri **tidak
dapat membetulkannya sendiri** — ia harus meminta supervisor membuka sesi koreksi. Itu memang
maksud `RWI-DEC-057`, tetapi alur kerjanya perlu dipastikan dapat dijalankan ruangan.

### 7.2 Tanda tangan tidak diperbarui saat amandemen

Setelah amandemen, `SignedAt` dan `SignedByDoctorId` **tetap** berisi tanda tangan yang lama.
Akibatnya, amandemen berikutnya juga melahirkan versi baru — yang benar, karena setiap koreksi
atas dokumen tertandatangani adalah amandemen tersendiri.

Bila Product/Domain menghendaki resume hasil amandemen ditandatangani ulang lebih dulu,
sebutkan — perilakunya berbeda dan konsekuensinya besar.

### 7.3 Sesi koreksi belum dapat dibuka lewat endpoint

Sama seperti dicatat laporan `BE-RWI-021` bagian 5.2: endpoint pembuka dan penutup sesi koreksi
milik `BE-RWI-030`. Sampai saat itu, jalur amandemen ini **tidak dapat dijalankan di
lingkungan sungguhan** — hanya dapat diuji dengan menyisipkan baris sesi koreksi langsung.

---

## 8. Validasi

### 8.1 Yang **belum** dijalankan

| Perintah | Keadaannya |
| --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | ✅ **PASS** — Build succeeded, 0 Error(s); dijalankan 26 Agustus 2026, dan diulang tiga kali berturut-turut dengan hasil sama |
| `dotnet test QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` | ✅ **PASS** — Passed! Failed 0, Passed 255, Skipped 0, Total 255 |
| `UAT-27` terhadap aplikasi berjalan | **NOT RUN** — menunggu `BE-RWI-030` |

### 8.2 Test yang ditulis

Di dalam `QuilvianSystemBackend.Tests/InPatientManagement/InpDischargeSummaryTests.cs`.

| Acceptance criteria | Test yang membuktikan | Status |
| --- | --- | --- |
| 1. Menyunting resume yang **belum** ditandatangani tidak membuat versi baru | `Kriteria1_ResumeDapatDisusunDanDiperbaruiSelagiBelumDitandatangani` — bagian akhirnya memeriksa daftar versi kosong | ✅ **Lulus** 26 Agu 2026 |
| 2. Mengubah resume tertandatangani lewat sesi koreksi menyimpan salinan versi sebelumnya | `Kriteria2_MengubahResumeTertandatanganLewatSesiKoreksiMenyimpanVersiSebelumnya` | ✅ **Lulus** 26 Agu 2026 |
| 3. Versi tersimpan tidak dapat diubah maupun dihapus | `Kriteria3_TidakAdaEndpointYangDapatMengubahAtauMenghapusVersiResume` | ✅ **Lulus** 26 Agu 2026 |
| 4. `includeRevisions=true` mengembalikan versi berlaku beserta daftar versi lama urut waktu | `Kriteria4_IncludeRevisionsMengembalikanVersiBerlakuBesertaDaftarVersiLamaUrutWaktu` — dua kali amandemen berturut-turut | ✅ **Lulus** 26 Agu 2026 |

Satu test tambahan menjaga bahwa supervisor adalah satu-satunya yang dapat mengubah resume
tertandatangani.

---

## 9. Dampak

| Aspek | Dampaknya |
| --- | --- |
| API contract | Parameter `includeRevisions` pada `GET .../summary` kini bekerja. Tidak ada endpoint baru |
| Database | Tidak ada perubahan schema; tabel versinya sudah dibuat `BE-RWI-003` |
| Keamanan | Isi versi resume bertanda sensitif; ia hanya muncul bila pemanggil meminta `includeRevisions` dan punya `InpatientDischarge : Read` |

---

## 10. Risiko yang tersisa

| Risiko | Akibatnya bila terwujud | Yang menutupnya |
| --- | --- | --- |
| **Build dan test belum dijalankan** | Keempat kriteria belum terbukti | Bagian 8.1 |
| Jalur amandemen belum dapat dijalankan sungguhan | Koreksi resume tertandatangani belum dapat dipakai ruangan | `BE-RWI-030` |
| Alur "DPJP tidak dapat membetulkan resumenya sendiri" belum dikonfirmasi | Supervisor menjadi hambatan pada koreksi yang seharusnya sederhana | Konfirmasi Product/Domain — bagian 7.1 |
| Perilaku tanda tangan setelah amandemen belum dikonfirmasi | Resume hasil koreksi beredar dengan tanda tangan yang mendahului isinya | Konfirmasi Product/Domain — bagian 7.2 |

---

## 11. Definition of Done

| Butir DoD | Keadaannya |
| --- | --- |
| Penyalinan versi aktif | ✅ Ada di dalam kode |
| Keempat kriteria lulus | ✅ **Lulus** — seluruh test-nya dijalankan 26 Agustus 2026 dan hijau (255/255) |
| Api contract diperbarui | ❌ **Belum, dan memang belum boleh** |

---

## 12. Langkah berikutnya

1. Jalankan `dotnet build` dan `dotnet test`.
2. Konfirmasi bagian 7.1 dan 7.2 ke Product/Domain.
3. Jalankan `UAT-27` setelah `BE-RWI-030` membuka sesi koreksi.
