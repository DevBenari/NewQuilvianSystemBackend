# Laporan Perubahan Backend — `BE-RWI-257`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-257` |
| Judul | Sumber ketiga dokter perujuk luar pada IPD |
| Slice | Repair 043 episode rawat inap; Slice 2 pada [MVP Penuntasan Rawat Inap](../../../../MVP-Penuntasan-Rawat-Inap.md) |
| Roadmap | [roadmap-repair-043.md](../../../roadmap/roadmap-repair-043.md), kartu `BE-RWI-257` |
| Trace | `FIX-EPS-043-05` menutup `ISS-EPS-043-03` bagian B; keputusan K-04 (2026-11-02) mengamandemen `RWI-DEC-254` dan `RWI-AC-386` |
| Contract version | Tidak mengubah bentuk API; hanya isi field `referringDoctor` pada data dasar IPD |
| Dependency | Tidak ada |
| Klasifikasi | `LIGHT` — satu cabang pada query baca dan dua field pada pembaca sumber |
| Task mode | `BACKEND` (ditambah wewenang dokumen blueprint) |
| Target tulis | `NewQuilvianSystemBackend` branch `MHamzah` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | Source masuk pada `da737fb9` (2026-11-02); diverifikasi pada HEAD `53418d372b6e37fa76aeabea9cf3b395c5534641` |
| Tanggal | 9 November 2026 |
| Status | ✅ Selesai: keempat acceptance criteria terpetakan ke source, `dotnet build` lulus |

---

## 1. Masalah yang diperbaiki

Lembar Data Dasar Rawat Inap (IPD) di Workspace PPRI mengambil nama "Dokter perujuk" dari surat
pengantar rawat inap. Bila tidak ada surat, sistem seharusnya membaca perujuk luar dari modul
Registrasi, tetapi service baca itu masih menunggu persetujuan pemilik Registrasi
(`RWI-OQ-128`, task `BE-RWI-190` ⛔). Akibatnya baris "Dokter perujuk" selalu kosong, padahal
petugas admisi sudah mengetik nama perujuk luar pada langkah admisi.

Contoh: Tn. C (samaran) datang membawa rujukan dr. D dari Klinik E. Petugas admisi mengetik
"dr. D" dan "Klinik E" pada form admisi. Lembar IPD tetap mencetak garis kosong.

---

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Lembar IPD mencetak nama perujuk yang sudah diketahui rumah sakit |
| Pelaku | Petugas admisi mencatat perujuk luar; petugas PPRI membuka dan mencetak IPD |
| Pemicu | Petugas membuka tab IPD pada Workspace PPRI |
| Prasyarat | Episode rawat inap sudah dibuat |

Urutan sumber nama perujuk (setelah amandemen K-04):

1. Surat pengantar rawat inap yang terbit pada kunjungan episode → nama dokter penerbit surat.
2. Perujuk luar dari modul Registrasi → saat ini selalu "belum tersedia" karena menunggu
   `RWI-OQ-128` (`InpAdmissionSourceReader.GetExternalReferralAsync`).
3. **Baru:** perujuk luar yang diketik admisi pada episode (`InpEpisode.ExternalReferralDoctorName`),
   ditambah nama fasilitas dalam kurung bila terisi.
4. Bila ketiganya kosong → garis kosong. Cetak IPD tidak pernah gagal karena hal ini.

| Kasus | Hasil cetak |
| --- | --- |
| Ada surat pengantar dari dr. G, admisi juga mengetik dr. D | "dr. G" (surat didahulukan) |
| Tanpa surat, admisi mengetik dr. D dari Klinik E | "dr. D (Klinik E)" |
| Tanpa surat, admisi mengetik dr. D tanpa fasilitas | "dr. D" |
| Tanpa surat dan tanpa perujuk luar | Garis kosong |

Jalur tidak normal: nama yang diketik bebas bisa salah eja; koreksinya memakai jalur koreksi
data episode yang sudah ada.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Plan repair 043 kartu `FIX-EPS-043-05` dan tabel keputusan K-04; roadmap repair 043;
`00-interview-decisions.md` (`RWI-DEC-254`, `RWI-AC-375`, `RWI-AC-386`);
`InpAdmissionWorkspaceQueryService.cs`; `InpAdmissionSourceReader.cs`; `InpEpisode.cs` baris 83-84.

### 3.2 Berkas yang berubah

Source sudah masuk commit `da737fb9`; **tidak ada perubahan source baru** pada sesi ini.

| Berkas (isi commit `da737fb9`) | Perubahan |
| --- | --- |
| `InPatientManagement/Services/InpAdmissionSourceReader.cs` | Pembaca episode ikut mengambil `ExternalReferralDoctorName` dan `ExternalReferralFacilityName` |
| `InPatientManagement/Services/InpAdmissionWorkspaceQueryService.cs` | Cabang ketiga pembentukan `referringDoctor`, sesudah surat pengantar dan perujuk luar Registrasi |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bentuk respons tidak berubah; field `referringDoctor` kini dapat terisi dari sumber ketiga |
| Database | Tidak ada. Kolom `ExternalReferralDoctorName` (150) dan `ExternalReferralFacilityName` (200) sudah ada pada `InpEpisode` |
| Keamanan/Auth | `NOT APPLICABLE` — endpoint dan hak akses IPD tidak berubah |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — tidak ada endpoint baru atau yang berubah bentuknya.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight | `InPatientManagement` / `Inp` (`ACTIVE`), `TOUCHED LEGACY`; QBE-SVC-001 (logika di service), QBE-DTO-001 | `PASS` | Review source |
| `dotnet build QuilvianSystemBackend.csproj -o <scratch>` (9 November 2026) | `0 Error(s)`, `12 Warning(s)` (peringatan paket NuGet lama) | `PASS` | Keluaran build |
| AC 1 — surat pengantar didahulukan | Cabang `letter.IsAvailable && letter.Value != null` dievaluasi pertama | `PASS` (penelusuran source) | `InpAdmissionWorkspaceQueryService.cs`, blok "Dokter perujuk" |
| AC 2 — "dr. D (Klinik E)" | Cabang ketiga menggabungkan nama dan fasilitas dalam kurung | `PASS` (penelusuran source) | Blok yang sama |
| AC 3 — "dr. D" tanpa fasilitas | Fasilitas kosong → hanya nama dokter | `PASS` (penelusuran source) | Blok yang sama |
| AC 4 — garis kosong tanpa galat | Ketiga cabang tidak terpenuhi → `referringDoctor` tetap `null`; layar mencetak garis kosong untuk nilai kosong seperti sebelumnya (`RWI-AC-375`) | `PASS` (penelusuran source) | Blok yang sama |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji runtime 9 November 2026 lewat `GET .../episodes/{episodeId}/admission-workspace/base-data`:

| Kasus | Episode | Hasil `referringDoctor` | Klasifikasi |
| --- | --- | --- | --- |
| AC 2 — tanpa surat, perujuk + fasilitas | `RI-261023085011-F0F277` | "dr. Uji Perujuk (Puskesmas Uji Agy)" | `PASS` |
| AC 2 — kasus kedua | `RI-261026104414-CEE503` | "dr. Ratih Wulandari (Puskesmas Kecamatan Setiabudi)" | `PASS` |
| AC 4 — tanpa surat dan tanpa perujuk | `RI-261028-VIP-081` | `null` (dicetak garis kosong), tanpa galat | `PASS` |
| AC 1 dan AC 3 | DB dev tidak punya episode dengan surat pengantar + perujuk luar, atau perujuk tanpa fasilitas | `NOT RUN` — terbukti di source | — |

Uji manual: `PASS` untuk AC 2 dan AC 4; AC 1 dan AC 3 terbukti di source.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC 1–4 `FIX-EPS-043-05` | Terpenuhi | Bagian 5 |
| DoD — `dotnet build` | Terpenuhi | `0 Error(s)` |
| DoD — laporan `task/report/backend/BE-RWI-257.md` | Terpenuhi | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Working tree backend memuat perubahan modul HR milik pekerjaan lain; tidak disentuh |
| Masalah yang diketahui | Amandemen `RWI-DEC-254` dan `RWI-AC-386` sesuai K-04 belum dicatat di `00-interview-decisions.md` (langkah 1 plan repair). Dicatat pada Slice 6 MVP |
| Risiko tersisa | Bila kelak `BE-RWI-190` selesai, perujuk luar Registrasi akan didahulukan atas ketikan admisi, sesuai urutan K-04 |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Tidak ada perubahan source; hanya dokumen blueprint rawat inap |
| Langkah berikutnya | Uji cetak IPD empat kasus pada sesi uji langsung |
